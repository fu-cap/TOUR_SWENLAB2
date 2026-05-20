using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using TourPlannerAPI.DTOs;
using TourPlannerAPI.Models;
using TourPlannerAPI.Repositories;

namespace TourPlannerAPI.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    private string HashPassword(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
        argon2.Salt = salt;
        argon2.DegreeOfParallelism = 8;
        argon2.MemorySize = 65536;
        argon2.Iterations = 4;
        var hash = argon2.GetBytes(32);
        return Convert.ToBase64String(hash);
    }

    private byte[] CreateSalt()
    {
        var buffer = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(buffer);
        return buffer;
    }

    public async Task<string> RegisterAsync(UserRegisterDto dto)
    {
        // Check if user exists
        var existingUser = await _userRepository.GetUserByUsernameAsync(dto.Username);
        if (existingUser != null)
        {
            throw new Exception("Username is already taken.");
        }

        var existingEmail = await _userRepository.GetUserByEmailAsync(dto.Email);
        if (existingEmail != null)
        {
            throw new Exception("Email is already registered.");
        }

        // Hash password
        var salt = CreateSalt();
        var passwordHash = HashPassword(dto.Password, salt);
        var combinedHash = $"{Convert.ToBase64String(salt)}:{passwordHash}";

        // Create user object
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = combinedHash,
            Gender = dto.Gender,
            Firstname = dto.Firstname,
            Lastname = dto.Lastname
        };

        // Save to DB
        await _userRepository.CreateUserAsync(user);

        // Return JWT or success message (returning token for immediate login)
        // Need to get the full user to get the ID for the token
        var createdUser = await _userRepository.GetUserByUsernameAsync(dto.Username);
        if (createdUser == null)
        {
            throw new Exception("Failed to retrieve created user.");
        }

        return GenerateJwtToken(createdUser);
    }

    public async Task<string> LoginAsync(UserLoginDto dto)
    {
        // Find user
        var user = await _userRepository.GetUserByUsernameAsync(dto.Username);
        if (user == null)
        {
            throw new Exception("Invalid username or password.");
        }

        // Verify password
        var parts = user.PasswordHash.Split(':');
        if (parts.Length != 2)
        {
            throw new Exception("Invalid username or password.");
        }
        var salt = Convert.FromBase64String(parts[0]);
        var expectedHash = parts[1];
        var actualHash = HashPassword(dto.Password, salt);

        if (expectedHash != actualHash)
        {
            throw new Exception("Invalid username or password.");
        }

        // Generate token
        return GenerateJwtToken(user);
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var keyString = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2), // Token valid for 2 hours
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
