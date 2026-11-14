using CourseManagement.Models;

namespace CourseManagement.Repositories.Interfaces;

public interface ICourseRepository
{
    Task<List<Course>> GetAllCourses();
    Task<Course?> FindCourseById(Guid id);

    Task<Course> AddCourse(Course course);

    Task<Course> UpdateCourse(Course course);

    Task<bool> DeleteCourse(Course course);

    Task<bool> CourseTitleExists (string title, Guid? excludeCourseId = null );

}