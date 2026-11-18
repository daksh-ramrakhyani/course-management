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

    private readonly IUserRepository userRepository;

    public CourseService(ICourseRepository repository, IUserRepository user)
    {
        courseRepository = repository;
        userRepository = user;
    }

    private CourseResponseDto MapCourse(Course course)
    {
        return new CourseResponseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Price = course.Price,
            DurationInHours = course.DurationInHours,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            TeacherId = course.TeacherId,
            TeacherName = course.Teacher.Name
        };
    }

    public async Task<Result<CourseResponseDto>> CreateCourse(CreateCourseDto dto, Guid TeacherId)
    {
        User? teacher = await userRepository.GetUserById(TeacherId);
        if (teacher == null)
        {
            return Result<CourseResponseDto>.Failure("User needs to be Authenticated", "AUTHENTICATION_ERROR");
        }

        if (teacher.Role != UserRoles.Teacher)
        {
            return Result<CourseResponseDto>.Failure(
                "Only teachers can create courses",
                "FORBIDDEN");
        }

        if (dto.DurationInHours < 1 || dto.DurationInHours > 30)
        {
            return Result<CourseResponseDto>.Failure("Duration must be in range 1 to 30", "VALIDATION_ERROR");
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return Result<CourseResponseDto>.Failure("Title must be a valid string", "VALIDATION_ERROR");
        }

        if (await courseRepository.CourseTitleExists(dto.Title.Trim()))
        {
            return Result<CourseResponseDto>.Failure("Course Title must be unique", "DUPLICATE_TITLE");
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            return Result<CourseResponseDto>.Failure("Description must be a valid string", "VALIDATION_ERROR");
        }

        if (dto.Price == null || dto.Price < 0 || dto.Price > 10000)
        {
            return Result<CourseResponseDto>.Failure("Price must be a positive number between 0 - 10000", "VALIDATION_ERROR");
        }

        Course course = new()
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Price = dto.Price.Value,
            DurationInHours = dto.DurationInHours,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            TeacherId = TeacherId,
        };

        Course courseCreated = await courseRepository.AddCourse(course);

        courseCreated.Teacher = teacher;

        return Result<CourseResponseDto>.Success("Course created successfully", MapCourse(courseCreated));
    }

    public async Task<Result<bool>> DeleteCourse(Guid id, Guid authenticatedUser, string role)
    {
        Course? course = await courseRepository.FindCourseById(id);
        if (course == null)
        {
            return Result<bool>.Failure("Course not found", "COURSE_NOT_FOUND");
        }
        if (course.TeacherId != authenticatedUser && role != "admin")
        {
            return Result<bool>.Failure("Not Authorised", "NOT_AUTHORISED");
        }
        await courseRepository.DeleteCourse(course);
        return Result<bool>.Success("Course deleted successfully", true);
    }

    public async Task<Result<CourseResponseDto?>> GetCourseById(Guid id)
    {
        Course? course = await courseRepository.FindCourseById(id);
        if (course == null)
        {
            return Result<CourseResponseDto?>.Failure("Course not found", "COURSE_NOT_FOUND");
        }

        return Result<CourseResponseDto?>.Success("Course found successfully", MapCourse(course));
    }

    public async Task<Result<List<CourseResponseDto>>> GetCourses()
    {
        List<Course> courses = await courseRepository.GetAllCourses();
        List<CourseResponseDto> courseList = courses.Select(MapCourse).ToList();

        return Result<List<CourseResponseDto>>.Success("Courses found successfully", courseList);
    }

    public async Task<Result<CourseResponseDto?>> UpdateCourse(Guid id, UpdateCourseDto dto, Guid authenticatedUser, string role)
    {
        // checking if course exist
        Course? course = await courseRepository.FindCourseById(id);
        if (course == null)
        {
            return Result<CourseResponseDto?>.Failure("Course not found", "COURSE_NOT_FOUND");
        }
        if (course.TeacherId != authenticatedUser && role != "admin")
        {
            return Result<CourseResponseDto?>.Failure("Not Authorised", "NOT_AUTHORISED");
        }

        bool isUpdated = false;

        // validation
        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            if (await courseRepository.CourseTitleExists(dto.Title.Trim(), id))
            {
                return Result<CourseResponseDto?>.Failure("Course Title must be unique", "DUPLICATE_TITLE");
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
            if (dto.DurationInHours < 1 || dto.DurationInHours > 30)
            {
                return Result<CourseResponseDto?>.Failure("Duration must be in range 1 to 30", "VALIDATION_ERROR");
            }

            course.DurationInHours = dto.DurationInHours.Value;
            isUpdated = true;
        }

        if (dto.Price.HasValue)
        {
            if (dto.Price == null || dto.Price < 0 || dto.Price > 10000)
            {
                return Result<CourseResponseDto?>.Failure("Price must be a positive number between 0 - 10000", "VALIDATION_ERROR");
            }

            course.Price = dto.Price.Value;
            isUpdated = true;
        }

        if (!isUpdated)
        {
            return Result<CourseResponseDto?>.Failure("Atleast one value is need to update", "VALIDATION_ERROR");
        }

        course.UpdatedAt = DateTime.UtcNow;

        await courseRepository.UpdateCourse(course);

        return Result<CourseResponseDto?>.Success("Course updated successfully", MapCourse(course));
    }
}