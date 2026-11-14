using System.ComponentModel.DataAnnotations;

namespace CourseManagement.DTOs;

public class UpdateCourseDto
{
    public string? Title { get; set; } 

    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int? DurationInHours { get; set; }

    [Range(0, 10000)]
    public decimal? Price { get; set; }
}