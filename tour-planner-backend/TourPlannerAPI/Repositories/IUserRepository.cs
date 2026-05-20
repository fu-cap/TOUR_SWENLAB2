using TourPlannerAPI.Models;

namespace TourPlannerAPI.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<Guid> CreateUserAsync(User user);
}
