
using CourseManagement.Data;
using CourseManagement.Models;
using CourseManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Repositories;

public class UserRepository : IUserRepository
{
    private readonly CourseDbContext courseDbContext;

    public UserRepository(CourseDbContext context)
    {
        courseDbContext = context;
    }

    public async Task<User> CreateUser(User user)
    {
        await courseDbContext
            .Database
            .ExecuteSqlInterpolatedAsync($"CALL CreateUser({user.Id},{user.Name},{user.EmailHash}, {user.EmailEncypted}, {user.EmailIV}, {user.EmailAuthTag}, {user.PasswordHash}, {user.Role}, {user.CreatedAt}, {user.UpdatedAt})");
        return user;
    }

    public async Task<bool> DeleteUser(User user)
    {
        var rowAffected = await courseDbContext.Database.ExecuteSqlInterpolatedAsync($"CALL DeleteUser({user.Id})");
        return rowAffected > 0;
    }

    public async Task<User?> GetUserById(Guid id)
    {
        var list = await courseDbContext.Users
            .FromSqlInterpolated($"CALL GetUserById({id})")
            .AsNoTracking()
            .ToListAsync();
        return list.FirstOrDefault();
    }

    public async Task<List<User>> GetUsers()
    {
        return await courseDbContext.Users.FromSqlRaw("CALL GetAllUsers()").ToListAsync();
    }

    public async Task<User?> GetUserByEmailHash(string EmailHash)
    {
        var userList = await courseDbContext.Users.FromSqlInterpolated($"CALL GetUserByEmailHash({EmailHash})").ToListAsync();
        return userList.FirstOrDefault();
    }

    public async Task<User> UpdateUser(User user)
    {
        await courseDbContext
            .Database
            .ExecuteSqlInterpolatedAsync($"CALL UpdateUser({user.Id}, {user.Name},{user.EmailHash},{user.EmailEncypted},{user.EmailIV},{user.EmailAuthTag},{user.UpdatedAt});");
        return user;
    }
}