using Application.DTOs;
using Application.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IUserService : IAutoDependencyService
    {
        Task<SuccessResponse<UserDto>> CreateUser(UserSignupDto model, List<string> roles = null);
        Task<SuccessResponse<object>> CompleteUserRegistration(VerifyTokenDTO token);
        Task<SuccessResponse<UserByIdDto>> GetUserById(Guid userId);
        Task<SuccessResponse<UserLoginResponse>> UserLogin(UserLoginDTO model);

        Task<SuccessResponse<object>> VerifyToken(VerifyTokenDTO model);
    }
}
