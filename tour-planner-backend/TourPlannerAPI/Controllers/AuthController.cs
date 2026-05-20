using Microsoft.AspNetCore.Mvc;
using TourPlannerAPI.DTOs;
using TourPlannerAPI.Services;

namespace TourPlannerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var token = await _authService.RegisterAsync(dto);
            return Ok(new TokenDto { Token = token });
        }
        catch (Exception ex)
        {
            // In a real application, you might want to return different status codes based on the exception type
            // e.g., Conflict if username/email already exists
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var token = await _authService.LoginAsync(dto);
            return Ok(new TokenDto { Token = token });
        }
        catch (Exception ex)
        {
            // e.g., Unauthorized for invalid credentials
            return Unauthorized(new { Message = ex.Message });
        }
    }
}
