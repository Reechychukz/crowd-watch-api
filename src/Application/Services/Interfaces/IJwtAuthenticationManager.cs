using Application.Helpers;
using Application.Services.Implementations;
using Domain.Entities.Identities;
using System;
using System.Collections.Generic;

namespace Application.Services.Interfaces
{
    public interface IJwtAuthenticationManager : IAutoDependencyService
    {
        TokenReturnHelper Authenticate(User user, IList<string> roles = null);
        Guid GetUserIdFromAccessToken(string accessToken);
        string GenerateRefreshToken(Guid userId);
        bool ValidateRefreshToken(string refreshToken);
    }
}
