namespace CourseManagement.Services.Interfaces;

using CourseManagement.DTOs;
using CourseManagement.Models;
using CourseManagement.Common;

public interface ICourseService
{
    Task<Result<List<Course>>> GetCourses();
    Task<Result<Course?>> GetCourseById(Guid id);

    Task<Result<Course>> CreateCourse(CreateCourseDto dto);

    Task<Result<Course?>> UpdateCourse(Guid id, UpdateCourseDto dto);

    Task<Result<bool>> DeleteCourse(Guid id);

}