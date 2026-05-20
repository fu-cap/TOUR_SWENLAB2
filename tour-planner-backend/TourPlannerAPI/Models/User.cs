namespace TourPlannerAPI.Models;

public class User
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Gender { get; set; }
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
    public DateTime CreatedAt { get; set; }
}
