using System.ComponentModel.DataAnnotations;

namespace TourPlannerAPI.DTOs;

public class UserRegisterDto
{
    [Required]
    [MinLength(3)]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MinLength(6)]
    public required string Password { get; set; }

    [Required]
    public required string Gender { get; set; }

    [Required]
    public required string Firstname { get; set; }

    [Required]
    public required string Lastname { get; set; }
}
