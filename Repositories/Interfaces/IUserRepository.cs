using CourseManagement.Models;

namespace CourseManagement.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User> CreateUser(User user);

    Task<User?> GetUserById(Guid id);

    Task<List<User>> GetUsers();

    Task<User> UpdateUser(User user);

    Task<bool> DeleteUser(User user);

    Task<User?> GetUserByEmailHash(string EmailHash);
}