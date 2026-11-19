using Dapper;
using Microsoft.AspNetCore.Mvc;
using PsychiatristAppointment.Context;
using PsychiatristAppointment.Models;
using System.Security.Cryptography;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly DapperContext _context;
    public AuthController(DapperContext context) => _context = context;

    private static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
    {
        using var hmac = new HMACSHA512();
        salt = hmac.Key;
        hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    private static bool VerifyPasswordHash(string password, byte[] hash, byte[] salt)
    {
        using var hmac = new HMACSHA512(salt);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(password)).SequenceEqual(hash);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        try
        {
            using var conn = _context.CreateConnection();
            var exists = await conn.QuerySingleAsync<int>("SELECT COUNT(1) FROM Users WHERE Email = @Email", new { dto.Email });
            if (exists > 0) return BadRequest(new { message = "User already exists" });

            CreatePasswordHash(dto.Password, out var hash, out var salt);
            
            // Adding defaults for columns that might exist in the database and have NOT NULL constraints
            await conn.ExecuteAsync(@"INSERT INTO Users (FullName, Email, PasswordHash, PasswordSalt, Role, CreatedAt, Specialization, IsActive, Fee) 
                                      VALUES (@FullName, @Email, @Hash, @Salt, @Role, @CreatedAt, @Specialization, @IsActive, @Fee)",
                                      new { 
                                          dto.FullName, 
                                          dto.Email, 
                                          Hash = hash, 
                                          Salt = salt, 
                                          dto.Role, 
                                          CreatedAt = DateTime.Now,
                                          Specialization = "",
                                          IsActive = true,
                                          Fee = 0m
                                      });
            return Ok(new { message = "Registered successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Registration failed: {ex.Message}" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        using var conn = _context.CreateConnection();
        var user = await conn.QuerySingleOrDefaultAsync<User>("SELECT * FROM Users WHERE Email = @Email", new { dto.Email });
        if (user == null || !VerifyPasswordHash(dto.Password, user.PasswordHash, user.PasswordSalt))
            return Unauthorized(new { message = "Invalid email or password" });

        return Ok(new { user.Id, user.FullName, user.Email, user.Role });
    }
}
