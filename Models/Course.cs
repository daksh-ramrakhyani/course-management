using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models;


public class Course
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(1, 30)]
    public int DurationInHours { get; set; }

    [Required]
    [Range(0, 10000)]
    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }


    public DateTime? UpdatedAt { get; set; }

}