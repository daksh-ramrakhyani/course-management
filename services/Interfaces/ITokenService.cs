using System.Security.Claims;
using CourseManagement.Models;

namespace CourseManagement.Services.Interfaces;

public interface ITokenService
{
    AuthTokensDto CreateTokens(User user);

    ClaimsPrincipal? ValidateRefreshToken(string token);
}