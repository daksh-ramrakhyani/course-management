using CourseManagement.Common;
using CourseManagement.Models;

namespace CourseManagement.Services.Interfaces;

public interface IUserService
{
    Task<Result<UserResponseDto?>> GetUserById(Guid id);

    Task<Result<List<UserResponseDto>>> GetAllUsers();

    Task<Result<UserResponseDto>> CreateUser(CreateUserDto dto);

    Task<Result<UserResponseDto?>> UpdateUser(UpdateUserDto dto, Guid id);

    Task<Result<AuthTokensDto>> LoginUser(LoginUserDto dto);

    Task<Result<AuthTokensDto>> RefreshTokens(Guid id);

    Task<Result<bool>> DeleteUser(Guid id);
}