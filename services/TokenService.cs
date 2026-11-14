using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CourseManagement.Models;
using CourseManagement.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace CourseManagement.Services;

public class TokenService : ITokenService
{
    private readonly byte[] accessKey;
    private readonly byte[] refreshKey;

    private const string Issuer = "CourseManagementAPI";
    private const string Audience = "CourseManagementClient";

    private const int AccessTokenMinutes = 15;
    private const int RefreshTokenDays = 7;

    public TokenService(IConfiguration configuration)
    {
        accessKey = GetKey(configuration["JWT_ACCESS_SECRET"]);
        refreshKey = GetKey(configuration["JWT_REFRESH_SECRET"]);
    }

    private static byte[] GetKey(string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("JWT secret is missing.");
        }

        byte[] key = Convert.FromBase64String(secret);

        if (key.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT secret must contain at least 32 bytes."
            );
        }

        return key;
    }

    public AuthTokensDto CreateTokens(User user)
    {
        DateTime now = DateTime.UtcNow;

        DateTime accessExpiry = now.AddMinutes(AccessTokenMinutes);
        DateTime refreshExpiry = now.AddDays(RefreshTokenDays);

        return new AuthTokensDto
        {
            User = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = string.Empty,
                Role = user.Role
            },

            AccessToken = GenerateToken(user, accessExpiry, accessKey, false),

            RefreshToken = GenerateToken(user, refreshExpiry, refreshKey, true),

            AccessTokenExpiresAt = accessExpiry,
            RefreshTokenExpiresAt = refreshExpiry
        };
    }

    private static string GenerateToken(
        User user,
        DateTime expiry,
        byte[] key,
        bool isRefreshToken)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(
                "token_type",
                isRefreshToken ? "refresh" : "access"
            )
        ];

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: expiry,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateRefreshToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = Issuer,

                    ValidateAudience = true,
                    ValidAudience = Audience,

                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    IssuerSigningKey = new SymmetricSecurityKey(refreshKey),

                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role,

                    ClockSkew = TimeSpan.FromSeconds(30)
                },
                out _
            );

            if (principal.FindFirst("token_type")?.Value != "refresh")
            {
                return null;
            }

            return principal;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}