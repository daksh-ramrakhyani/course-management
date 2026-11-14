using System.Security.Claims;
using CourseManagement.Models;
using CourseManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagement.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService userService;
    private readonly ITokenService tokenService;

    public UserController(
        IUserService service,
        ITokenService tokens)
    {
        userService = service;
        tokenService = tokens;
    }

    // COOKIE HELPERS

    private CookieOptions CreateCookieOptions(
        DateTime expires,
        string path)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = expires,
            Path = path,
            IsEssential = true
        };
    }

    private void SetAuthCookies(AuthTokensDto tokens)
    {
        Response.Cookies.Append(
            "accessToken",
            tokens.AccessToken,
            CreateCookieOptions(tokens.AccessTokenExpiresAt, "/")
        );

        Response.Cookies.Append(
            "refreshToken",
            tokens.RefreshToken,
            CreateCookieOptions(tokens.RefreshTokenExpiresAt, "/User")
        );
    }

    private void ClearAuthCookies()
    {
        Response.Cookies.Delete(
            "accessToken",
            CreateCookieOptions(DateTime.UtcNow.AddDays(-1), "/")
        );

        Response.Cookies.Delete(
            "refreshToken",
            CreateCookieOptions(DateTime.UtcNow.AddDays(-1), "/User")
        );
    }

    private bool CanAccessUser(Guid id)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        string? currentUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(currentUserId, out Guid parsedId)
               && parsedId == id;
    }

    // REGISTER

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> AddUser([FromBody]CreateUserDto dto)
    {
        var result = await userService.CreateUser(dto);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "DUPLICATE_EMAIL")
                return Conflict(result.Message);

            if (result.ErrorCode == "VALIDATION_ERROR")
                return BadRequest(result.Message);

            return StatusCode(500, result.Message);
        }

        return CreatedAtAction(
            nameof(GetUserById),
            new { id = result.Data!.Id },
            result.Data
        );
    }

    // LOGIN

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginUserDto dto)
    {
        var result = await userService.LoginUser(dto);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "VALIDATION_ERROR")
                return BadRequest(result.Message);

            return Unauthorized(result.Message);
        }

        SetAuthCookies(result.Data!);

        return Ok(new
        {
            message = result.Message,
            user = result.Data!.User
        });
    }

    // REFRESH ACCESS TOKEN

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue(
            "refreshToken",
            out string? refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized("Refresh token missing.");
        }

        var principal = tokenService.ValidateRefreshToken(refreshToken);

        if (principal == null)
        {
            ClearAuthCookies();
            return Unauthorized("Invalid or expired refresh token.");
        }

        string? userId =
            principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out Guid id))
        {
            ClearAuthCookies();
            return Unauthorized("Invalid refresh token.");
        }

        var result = await userService.RefreshTokens(id);

        if (!result.IsSuccess)
        {
            ClearAuthCookies();
            return Unauthorized(result.Message);
        }

        SetAuthCookies(result.Data!);

        return Ok(new
        {
            message = result.Message,
            user = result.Data!.User
        });
    }

    // LOGOUT

    [HttpPost("logout")]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        ClearAuthCookies();

        return Ok("Logged out successfully.");
    }

    // GET ALL USERS - ADMIN ONLY

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers()
    {
        var result = await userService.GetAllUsers();

        return Ok(result.Data);
    }

    // GET USER BY ID - OWN PROFILE OR ADMIN

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        if (!CanAccessUser(id))
            return Forbid();

        var result = await userService.GetUserById(id);

        if (!result.IsSuccess)
            return NotFound(result.Message);

        return Ok(result.Data);
    }

    // UPDATE USER - OWN PROFILE OR ADMIN

    [HttpPatch("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        UpdateUserDto dto)
    {
        if (!CanAccessUser(id))
            return Forbid();

        var result = await userService.UpdateUser(dto, id);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "USER_NOT_FOUND")
                return NotFound(result.Message);

            if (result.ErrorCode == "DUPLICATE_EMAIL")
                return Conflict(result.Message);

            return BadRequest(result.Message);
        }

        return Ok(result.Data);
    }

    // DELETE USER - ADMIN ONLY

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await userService.DeleteUser(id);

        if (!result.IsSuccess)
            return NotFound(result.Message);

        return NoContent();
    }
}