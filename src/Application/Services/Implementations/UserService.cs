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
using Microsoft.Extensions.Configuration;
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
        private readonly IRepository<Token> _tokenRepository;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly RoleManager<Role> _roleManager;
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;
        public UserService(IRepository<User> userRepository, IRepository<UserActivity> userActivityRepository, UserManager<User> userManager, IMapper mapper, RoleManager<Role> roleManager, IJwtAuthenticationManager jwtAuthenticationManager, IRepository<Token> tokenRepository, IEmailTemplateService emailTemplateService, IConfiguration configuration, IMailService mailService)
        {
            _userRepository = userRepository;
            _userActivityRepository = userActivityRepository;
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
            _jwtAuthenticationManager = jwtAuthenticationManager;
            _tokenRepository = tokenRepository;
            _emailTemplateService = emailTemplateService;
            _configuration = configuration;
            _mailService = mailService;
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

            var token = CustomToken.GenerateOtp();

            //send comfirmation token using a fake smtp fake
            var tokenEntity = new Token
            {
                UserId = user.Id,
                TokenType = TokenTypeEnum.Email_Confirmation,
                OTPToken = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                IsValid = true
            };
            await _tokenRepository.AddAsync(tokenEntity);

            const string title = "Confirm Email Address";
            var message = _emailTemplateService.GetConfirmEmailTemplate(token, user.Email, user.FirstName, title);
            await _mailService.SendSingleMail(user.Email, message, title);

            await _userActivityRepository.SaveChangesAsync();

            var userResponse = _mapper.Map<UserDto>(user);

            return new SuccessResponse<UserDto>
            {
                Message = ResponseMessages.CreationSuccessResponse,
                Data = userResponse
            };
        }

        public async Task<SuccessResponse<object>> CompleteUserRegistration(VerifyTokenDTO token)
        {
            var verifyToken = await VerifyToken(token);
            if (!verifyToken.Success)
                throw new RestException(HttpStatusCode.BadRequest, message: ResponseMessages.InvalidToken);

            var tokenEntity = await _tokenRepository.FirstOrDefault(x => x.OTPToken == token.Token);
            if (tokenEntity.ExpiresAt < DateTime.UtcNow)
                throw new RestException(HttpStatusCode.BadRequest, message: ResponseMessages.ExpiredToken);

            var user = await _userRepository.FirstOrDefault(x => x.Id == tokenEntity.UserId);
            if (user.EmailConfirmed == true)
                throw new RestException(HttpStatusCode.BadRequest, message: ResponseMessages.EmailAlreadyVerified);

            user.EmailConfirmed = true;
            user.Verified = true;
            _userRepository.Update(user);

            await _userRepository.SaveChangesAsync();

            return new SuccessResponse<object>
            {
                Message = ResponseMessages.EmailVerificationSuccessResponse
            };
        }

        public async Task<SuccessResponse<UserLoginResponse>> UserLogin(UserLoginDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                throw new RestException(HttpStatusCode.NotFound, ResponseMessages.WrongEmailOrPassword);

            //ReSharper disable once HeapView.BoxingAllocation
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

        public async Task<SuccessResponse<object>> VerifyToken(VerifyTokenDTO model)
        {
            var token = await _tokenRepository.FirstOrDefault(x => x.OTPToken == model.Token);
            if (token == null)
                throw new RestException(HttpStatusCode.NotFound, ResponseMessages.InvalidToken);

            if (!token.IsValid)
                throw new RestException(HttpStatusCode.BadRequest, "Sorry, token has been invalidated.");

            var tokenLifeSpan = double.Parse(_configuration["TOKEN_LIFESPAN"]);

            var isValid = CustomToken.IsTokenValid(token, tokenLifeSpan);
            if (!isValid)
                throw new RestException(HttpStatusCode.NotFound, ResponseMessages.InvalidToken);

            return new SuccessResponse<object>
            {
                Message = ResponseMessages.TokenVerificationSuccessResponse
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
