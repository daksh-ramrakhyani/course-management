namespace CourseManagement.Models;

public class AuthTokensDto
{
    public UserResponseDto User { get; set; } = new();

    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime AccessTokenExpiresAt { get; set; }

    public DateTime RefreshTokenExpiresAt { get; set; }
}