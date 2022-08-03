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
        Task<SuccessResponse<UserByIdDto>> GetUserById(Guid userId);
    }
}
