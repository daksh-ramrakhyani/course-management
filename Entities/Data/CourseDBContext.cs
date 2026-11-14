using CourseManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Data;

public class CourseDbContext : DbContext
{
    public DbSet<Course> Courses  {get; set;} = null!;
    public DbSet<User> Users {get; set;} = null!; 

    public CourseDbContext (DbContextOptions<CourseDbContext> options) : base(options) 
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Course>()
            .HasIndex(c => c.Title)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.EmailHash)
            .IsUnique();
    }
}