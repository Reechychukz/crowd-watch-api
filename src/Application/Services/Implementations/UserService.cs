using Application.DTOs;
using Application.Helpers;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.Identities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Application.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserActivity> _userActivityRepository;
        private readonly IRepository<UserFriend> _userFriendRepository;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly RoleManager<Role> _roleManager;
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;

        public UserService(IRepository<User> userRepository, IRepository<UserActivity> userActivityRepository, UserManager<User> userManager, IMapper mapper, RoleManager<Role> roleManager, IJwtAuthenticationManager jwtAuthenticationManager, IRepository<UserFriend> userFriendRepository)
        {
            _userRepository = userRepository;
            _userActivityRepository = userActivityRepository;
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
            _jwtAuthenticationManager = jwtAuthenticationManager;
            _userFriendRepository = userFriendRepository;
        }

        public async Task<SuccessResponse<UserDto>> CreateUser(UserSignupDto model, List<string> roles = null)
        {
            var email = model.Email.Trim().ToLower();
            var isEmailExist = await _userRepository.ExistsAsync(x => x.Email == email);

            if (isEmailExist)
                throw new RestException(HttpStatusCode.BadRequest, message: ResponseMessages.DuplicateEmail);

            if (string.IsNullOrEmpty(model.Password))
                throw new RestException(HttpStatusCode.BadGateway, message: ResponseMessages.PasswordCannotBeEmpty);

            var user = _mapper.Map<User>(model);
            if (roles is null)
            {
                roles = new List<string>();
                var role = await _roleManager.FindByNameAsync(ERole.USER.ToString());
                if (role is not null)
                    roles.Add(role.Name);
            }

            await _userManager.CreateAsync(user, model.Password);

            await AddUserToRoles(user, roles);

            var userActivity = new UserActivity
            {
                EventType = "User created",
                UserId = user.Id,
                ObjectClass = "USER",
                Details = "signed up",
                ObjectId = user.Id
            };

            await _userActivityRepository.AddAsync(userActivity);

            await _userActivityRepository.SaveChangesAsync();

            var userResponse = _mapper.Map<UserDto>(user);

            return new SuccessResponse<UserDto>
            {
                Message = ResponseMessages.CreationSuccessResponse,
                Data = userResponse
            };
        }

        public async Task<SuccessResponse<UserLoginResponse>> UserLogin(UserLoginDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                throw new RestException(HttpStatusCode.NotFound, ResponseMessages.WrongEmailOrPassword);

            // ReSharper disable once HeapView.BoxingAllocation
            if (!user.EmailConfirmed || user?.Status?.ToUpper() != EUserStatus.ACTIVE.ToString() || !user.Verified)
                throw new RestException(HttpStatusCode.NotFound, ResponseMessages.WrongEmailOrPassword);

            var isUserValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isUserValid)
                throw new RestException(HttpStatusCode.NotFound, ResponseMessages.WrongEmailOrPassword);

            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var userActivity = new UserActivity
            {
                EventType = "User Login",
                UserId = user.Id,
                ObjectClass = "USER",
                Details = "logged in",
                ObjectId = user.Id
            };

            var roles = await _userManager.GetRolesAsync(user);
            await _userActivityRepository.AddAsync(userActivity);
            await _userActivityRepository.SaveChangesAsync();

            var userViewModel = _mapper.Map<UserLoginResponse>(user);

            var tokenResponse = _jwtAuthenticationManager.Authenticate(user, roles);
            userViewModel.AccessToken = tokenResponse.AccessToken;
            userViewModel.ExpiresIn = tokenResponse.ExpiresIn;
            userViewModel.RefreshToken = _jwtAuthenticationManager.GenerateRefreshToken(user.Id);

            return new SuccessResponse<UserLoginResponse>
            {
                Message = ResponseMessages.LoginSuccessResponse,
                Data = userViewModel
            };
        }

        public async Task<SuccessResponse<UserByIdDto>> GetUserById(Guid userId)
        {
            var user = await _userRepository.SingleOrDefaultNoTracking(x => x.Id == userId);

            if (user == null)
                throw new RestException(HttpStatusCode.NotFound, ResponseMessages.UserNotFound);

            var userResponse = _mapper.Map<UserByIdDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            userResponse.Roles = new List<string>();

            foreach (var role in roles)
                userResponse.Roles.Add(role);

            return new SuccessResponse<UserByIdDto>
            {
                Message = ResponseMessages.RetrievalSuccessResponse,
                Data = userResponse
            };
        }

        public async Task<Response> AddFriend(string friendEmailAddress)
        {
            var user = await _userRepository.GetByIdAsync(WebHelper.UserId);
            var friendUser = await _userRepository.FirstOrDefault(x => x.Email == friendEmailAddress);

            var newFiendRequest = new UserFriend
            {
                RequestedById = WebHelper.UserId,
                RequestedToId = friendUser.Id,
                RequestedBy = user,
                RequestedTo = friendUser,
                RequestTime = DateTime.UtcNow,
                FriendRequestFlag = EFriendRequestFlag.PENDING,
            };

            user.SentFriendRequests.Add(newFiendRequest);
            friendUser.ReceievedFriendRequests.Add(newFiendRequest);

            await _userFriendRepository.SaveChangesAsync();

            return new Response
            {
                Message = ResponseMessages.RetrievalSuccessResponse,
                Success = true
            };
        }

        #region Private Functions
        private async Task AddUserToRoles(User user, IEnumerable<string> roles)
        {
            foreach (var role in roles)
            {
                if (!await _userManager.IsInRoleAsync(user, role))
                    await _userManager.AddToRoleAsync(user, role);
            }
        }
        #endregion
    }
}
