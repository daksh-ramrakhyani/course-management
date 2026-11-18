namespace CourseManagement.Services.Interfaces;

using CourseManagement.DTOs;
using CourseManagement.Models;
using CourseManagement.Common;

public interface ICourseService
{
    Task<Result<List<CourseResponseDto>>> GetCourses();
    Task<Result<CourseResponseDto?>> GetCourseById(Guid id);

    Task<Result<CourseResponseDto>> CreateCourse(CreateCourseDto dto, Guid TeacherId);

    Task<Result<CourseResponseDto?>> UpdateCourse(Guid id, UpdateCourseDto dto, Guid authenticatedUser, string role);

    Task<Result<bool>> DeleteCourse(Guid id, Guid authenticatedUser, string role);

}