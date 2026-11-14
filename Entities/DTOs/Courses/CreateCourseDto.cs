using System.ComponentModel.DataAnnotations;

namespace CourseManagement.DTOs;

public class CreateCourseDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int DurationInHours { get; set; }

    [Required]
    [Range(0, 10000)]
    public decimal? Price { get; set; }

}