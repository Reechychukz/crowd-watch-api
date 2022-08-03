using Application.DTOs;
using Application.Helpers;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Common;
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
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly RoleManager<Role> _roleManager;

        public UserService(IRepository<User> userRepository, IRepository<UserActivity> userActivityRepository, UserManager<User> userManager, IMapper mapper, RoleManager<Role> roleManager)
        {
            _userRepository = userRepository;
            _userActivityRepository = userActivityRepository;
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
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
