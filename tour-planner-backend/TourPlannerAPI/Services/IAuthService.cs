using TourPlannerAPI.DTOs;
using TourPlannerAPI.Models;

namespace TourPlannerAPI.Services;

public interface IAuthService
{
    Task<string> RegisterAsync(UserRegisterDto dto);
    Task<string> LoginAsync(UserLoginDto dto);
}
