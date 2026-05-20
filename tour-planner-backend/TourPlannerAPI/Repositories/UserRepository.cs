using Dapper;
using Npgsql;
using TourPlannerAPI.Models;
using Microsoft.Extensions.Configuration;

namespace TourPlannerAPI.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IConfiguration _configuration;

    public UserRepository(IConfiguration configuration)
    {
        _configuration = configuration;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    private string GetConnectionString()
    {
        return _configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        using var connection = new NpgsqlConnection(GetConnectionString());
        const string query = "SELECT * FROM app_user WHERE username = @Username";
        return await connection.QuerySingleOrDefaultAsync<User>(query, new { Username = username });
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        using var connection = new NpgsqlConnection(GetConnectionString());
        const string query = "SELECT * FROM app_user WHERE email = @Email";
        return await connection.QuerySingleOrDefaultAsync<User>(query, new { Email = email });
    }

    public async Task<Guid> CreateUserAsync(User user)
    {
        using var connection = new NpgsqlConnection(GetConnectionString());
        const string query = @"
            INSERT INTO app_user (username, email, password_hash, gender, firstname, lastname)
            VALUES (@Username, @Email, @PasswordHash, @Gender, @Firstname, @Lastname)
            RETURNING id;";

        return await connection.ExecuteScalarAsync<Guid>(query, user);
    }
}
