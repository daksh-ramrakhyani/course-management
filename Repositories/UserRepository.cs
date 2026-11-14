
using CourseManagement.Data;
using CourseManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Repositories;

public class UserRepository : IUserRepository
{
    private readonly CourseDbContext courseDbContext;

    public UserRepository(CourseDbContext context) {
        courseDbContext = context;
    }

    public async Task<User> CreateUser(User user)
    {
        await courseDbContext.Users.AddAsync(user);
        await courseDbContext.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUser(User user)
    {
        courseDbContext.Users.Remove(user);
        await courseDbContext.SaveChangesAsync();
        return true;
    }

    public async Task<User?> GetUserById(Guid id)
    {
        return await courseDbContext.Users.FindAsync(id);
    }

    public async Task<List<User>> GetUsers()
    {
        return await courseDbContext.Users.ToListAsync();
    }

    public async Task<User?> GetUserByEmailHash(string EmailHash)
    {
        return await courseDbContext.Users.FirstOrDefaultAsync(u => u.EmailHash == EmailHash);

    }
 
    public async Task<User> UpdateUser(User user)
    {
        await courseDbContext.SaveChangesAsync();
        return user;
    }
}