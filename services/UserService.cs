using CourseManagement.Common;
using CourseManagement.Models;
using CourseManagement.Repositories;
using CourseManagement.Repositories.Interfaces;
using CourseManagement.Services.Interfaces;
using CourseManagement.Utils;
using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Services;

public class UserService : IUserService
{
    private readonly IUserRepository userRepository;
    private readonly ITokenService tokenService;

    public UserService(
        IUserRepository repository,
        ITokenService tokens)
    {
        userRepository = repository;
        tokenService = tokens;
    }

    // Convert database entity into safe API response DTO
    private UserResponseDto MapUser(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = EmailSecurity.DecryptEmail(
                user.EmailEncypted,
                user.EmailIV,
                user.EmailAuthTag
            ),
            Role = user.Role
        };
    }

    // REGISTER
    public async Task<Result<UserResponseDto>> CreateUser(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<UserResponseDto>.Failure(
                "Name cannot be empty.",
                "VALIDATION_ERROR"
            );
        }

        if (string.IsNullOrWhiteSpace(dto.Email) ||
            !new EmailAddressAttribute().IsValid(dto.Email))
        {
            return Result<UserResponseDto>.Failure(
                "Please provide a valid email.",
                "VALIDATION_ERROR"
            );
        }

        if (string.IsNullOrWhiteSpace(dto.Password) ||
            dto.Password.Length < 8)
        {
            return Result<UserResponseDto>.Failure(
                "Password must contain at least 8 characters.",
                "VALIDATION_ERROR"
            );
        }

        string emailHash = EmailSecurity.HashEmail(dto.Email);

        User? existingUser =
            await userRepository.GetUserByEmailHash(emailHash);

        if (existingUser != null)
        {
            return Result<UserResponseDto>.Failure(
                "User already exists.",
                "DUPLICATE_EMAIL"
            );
        }

        var encryptedEmail = EmailSecurity.EncryptEmail(dto.Email);

        User user = new()
        {
            Name = dto.Name.Trim(),
            EmailHash = emailHash,
            EmailEncypted = encryptedEmail.EncryptedEmail,
            EmailIV = encryptedEmail.IV,
            EmailAuthTag = encryptedEmail.AuthTag,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),

            // Never trust a role sent by the client
            Role = dto.Role,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        User newUser = await userRepository.CreateUser(user);

        return Result<UserResponseDto>.Success(
            "User registered successfully.",
            MapUser(newUser)
        );
    }

    // LOGIN
    public async Task<Result<AuthTokensDto>> LoginUser(LoginUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return Result<AuthTokensDto>.Failure(
                "Email and password are required.",
                "VALIDATION_ERROR"
            );
        }

        string emailHash = EmailSecurity.HashEmail(dto.Email);

        User? user =
            await userRepository.GetUserByEmailHash(emailHash);

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return Result<AuthTokensDto>.Failure(
                "Invalid email or password.",
                "INVALID_CREDENTIALS"
            );
        }

        AuthTokensDto tokens = tokenService.CreateTokens(user);

        // Include the decrypted email in the user response
        tokens.User = MapUser(user);

        return Result<AuthTokensDto>.Success(
            "Login Successfull.",
            tokens
        );
    }

    // REFRESH TOKENS
    public async Task<Result<AuthTokensDto>> RefreshTokens(Guid id)
    {
        User? user = await userRepository.GetUserById(id);

        if (user == null)
        {
            return Result<AuthTokensDto>.Failure(
                "User not found.",
                "USER_NOT_FOUND"
            );
        }

        AuthTokensDto tokens = tokenService.CreateTokens(user);
        tokens.User = MapUser(user);

        return Result<AuthTokensDto>.Success(
            "Tokens refreshed successfully.",
            tokens
        );
    }

    // GET ALL USERS
    public async Task<Result<List<UserResponseDto>>> GetAllUsers()
    {
        List<User> users = await userRepository.GetUsers();

        List<UserResponseDto> usersList =
            users.Select(MapUser).ToList();

        return Result<List<UserResponseDto>>.Success(
            "Users fetched successfully.",
            usersList
        );
    }

    // GET USER BY ID
    public async Task<Result<UserResponseDto?>> GetUserById(Guid id)
    {
        User? user = await userRepository.GetUserById(id);

        if (user == null)
        {
            return Result<UserResponseDto?>.Failure(
                "User not found.",
                "USER_NOT_FOUND"
            );
        }

        return Result<UserResponseDto?>.Success(
            "User found successfully.",
            MapUser(user)
        );
    }

    // UPDATE USER
    public async Task<Result<UserResponseDto?>> UpdateUser(
        UpdateUserDto dto,
        Guid id)
    {
        User? user = await userRepository.GetUserById(id);

        if (user == null)
        {
            return Result<UserResponseDto?>.Failure(
                "User not found.",
                "USER_NOT_FOUND"
            );
        }

        if (dto.Name != null)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return Result<UserResponseDto?>.Failure(
                    "Name cannot be empty.",
                    "VALIDATION_ERROR"
                );
            }

            user.Name = dto.Name.Trim();
        }

        if (dto.Email != null)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) ||
                !new EmailAddressAttribute().IsValid(dto.Email))
            {
                return Result<UserResponseDto?>.Failure(
                    "Please provide a valid email.",
                    "VALIDATION_ERROR"
                );
            }

            string newEmailHash = EmailSecurity.HashEmail(dto.Email);

            User? existingUser =
                await userRepository.GetUserByEmailHash(newEmailHash);

            if (existingUser != null && existingUser.Id != id)
            {
                return Result<UserResponseDto?>.Failure(
                    "Email is already registered.",
                    "DUPLICATE_EMAIL"
                );
            }

            var encryptedEmail = EmailSecurity.EncryptEmail(dto.Email);

            user.EmailHash = newEmailHash;
            user.EmailEncypted = encryptedEmail.EncryptedEmail;
            user.EmailIV = encryptedEmail.IV;
            user.EmailAuthTag = encryptedEmail.AuthTag;
        }

        user.UpdatedAt = DateTime.UtcNow;

        await userRepository.UpdateUser(user);

        return Result<UserResponseDto?>.Success(
            "User updated successfully.",
            MapUser(user)
        );
    }

    // DELETE USER
    public async Task<Result<bool>> DeleteUser(Guid id)
    {
        User? user = await userRepository.GetUserById(id);

        if (user == null)
        {
            return Result<bool>.Failure(
                "User not found.",
                "USER_NOT_FOUND"
            );
        }

        await userRepository.DeleteUser(user);

        return Result<bool>.Success(
            "User deleted successfully.",
            true
        );
    }
}