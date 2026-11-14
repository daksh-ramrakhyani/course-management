
using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models;


public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string EmailHash { get; set; } = string.Empty;

    [Required]
    public string EmailEncypted { get; set; } = string.Empty;

    [Required]
    public string EmailIV { get; set; } = string.Empty;

    [Required]
    public string EmailAuthTag { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRoles Role { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Required]
    public DateTime UpdatedAt { get; set; }

}