namespace CourseManagement.Repositories;

using CourseManagement.Models;
using CourseManagement.Data;
using Microsoft.EntityFrameworkCore;
using CourseManagement.Repositories.Interfaces;

public class CourseRepository : ICourseRepository
{

    private readonly CourseDbContext courseDbContext;

    public CourseRepository(CourseDbContext context)
    {
        this.courseDbContext = context;
    }


    public async Task<Course> AddCourse(Course course)
    {
        await courseDbContext.Courses.AddAsync(course);
        await courseDbContext.SaveChangesAsync();
        
        return course;
    }

    public async Task<bool> CourseTitleExists(string title, Guid? excludeCourseId = null)
    {
        if (excludeCourseId != null)
        {
            return await courseDbContext.Courses.AnyAsync(c => c.Title == title && c.Id != excludeCourseId);
        }
        return await courseDbContext.Courses.AnyAsync(c => c.Title == title);
    }

    public async Task<bool> DeleteCourse(Course course)
    {
        courseDbContext.Courses.Remove(course);
        await courseDbContext.SaveChangesAsync();
        return true;
    }

    public async Task<Course?> FindCourseById(Guid id)
    {
        return await courseDbContext.Courses
                                    .Include(u => u.Teacher)
                                    .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Course>> GetAllCourses()
    {
        return await courseDbContext.Courses
                        // .FromSqlRaw("Call GetAllCourses()")
                        .Include(u => u.Teacher)
                        .ToListAsync();
    }

    public async Task<Course> UpdateCourse(Course course)
    {
        await courseDbContext.SaveChangesAsync();
        return course;
    }
}