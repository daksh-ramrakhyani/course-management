using System.ComponentModel.DataAnnotations;

namespace CourseManagement.DTOs;

public class CourseResponseDto
{

    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int DurationInHours { get; set; }

    [Range(0, 10000)]
    public decimal Price { get; set; }

    public Guid TeacherId { get; set; }

    public string TeacherName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

}