namespace CourseManagement.Services;

using CourseManagement.Common;
using CourseManagement.DTOs;
using CourseManagement.Models;
using CourseManagement.Services.Interfaces;
using CourseManagement.Repositories.Interfaces;

// using CourseManagement.Repositories;

public class CourseService : ICourseService
{

    private readonly ICourseRepository courseRepository;

    public CourseService(ICourseRepository repository)
    {
        courseRepository = repository;
    }


    public async Task<Result<Course>> CreateCourse(CreateCourseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return Result<Course>.Failure("Title must be a valid string", "VALIDATION_ERROR");
        }

        if (await courseRepository.CourseTitleExists(dto.Title.Trim()))
        {
            return Result<Course>.Failure("Course Title must be unique", "DUPLICATE_TITLE");
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            return Result<Course>.Failure("Description must be a valid string", "VALIDATION_ERROR");
        }

        if(dto.Price == null)
        {
            return Result<Course>.Failure("Price must be a positive number", "VALIDATION_ERROR");
        }

        Course course = new()
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Price = dto.Price.Value,
            DurationInHours = dto.DurationInHours,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow

        };

        await courseRepository.AddCourse(course);

        return Result<Course>.Success("Course created successfully", course);


    }

    public async Task<Result<bool>> DeleteCourse(Guid id)
    {
        Course? course = await courseRepository.FindCourseById(id);
        if (course == null)
        {
            return Result<bool>.Failure("Course not found", "COURSE_NOT_FOUND");
        }
        await courseRepository.DeleteCourse(course);
        return Result<bool>.Success("Course deleted successfully", true);
    }

    public async Task<Result<Course?>> GetCourseById(Guid id)
    {
        Course? course = await courseRepository.FindCourseById(id);
        if (course == null)
        {
            return Result<Course?>.Failure("Course not found", "COURSE_NOT_FOUND");
        }
        return Result<Course?>.Success("Course found successfully", course);
    }

    public async Task<Result<List<Course>>> GetCourses()
    {
        List<Course> courses = await courseRepository.GetAllCourses();

        return Result<List<Course>>.Success("Courses found successfully", courses);

    }

    public async Task<Result<Course?>> UpdateCourse(Guid id, UpdateCourseDto dto)
    {
        // checking if course exist
        Course? course = await courseRepository.FindCourseById(id);
        if (course == null)
        {
            return Result<Course?>.Failure("Course not found", "COURSE_NOT_FOUND");
        }

        bool isUpdated = false;

        // validation
        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            if (await courseRepository.CourseTitleExists(dto.Title.Trim(), id))
            {
                return Result<Course?>.Failure("Course Title must be unique", "DUPLICATE_TITLE");
            }
            course.Title = dto.Title.Trim();
            isUpdated = true;
        }

        if (!string.IsNullOrWhiteSpace(dto.Description))
        {
            course.Description = dto.Description.Trim();
            isUpdated = true;
        }

        if (dto.DurationInHours.HasValue)
        {
            course.DurationInHours = dto.DurationInHours.Value;
            isUpdated = true;
        }

        if (dto.Price.HasValue)
        {
            course.Price = dto.Price.Value;
            isUpdated = true;
        }

        if (!isUpdated)
        {
            return Result<Course?>.Failure("Atleast one value is need to update", "VALIDATION_ERROR");
        }

        course.UpdatedAt = DateTime.UtcNow;

        await courseRepository.UpdateCourse(course);

        return Result<Course?>.Success("Course updated successfully", course);
    }
}