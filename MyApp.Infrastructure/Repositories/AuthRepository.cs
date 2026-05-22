using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MyApp.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IConfiguration _config;

        public AuthRepository(IConfiguration config) => _config = config;

        private IDbConnection CreateConnection()
            => new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

        private static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private string GenerateToken(string username, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"]!)),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<int> RegisterAsync(RegisterDto dto)
        {
            using var conn = CreateConnection();
            var exists = await conn.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM users WHERE username = @Username)",
                new { dto.Username });

            if (exists) throw new InvalidOperationException("Username already exists.");

            return await conn.ExecuteAsync(
                "INSERT INTO users (username, password_hash, role) VALUES (@Username, @PasswordHash, @Role)",
                new { dto.Username, PasswordHash = HashPassword(dto.Password), dto.Role });
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
        {
            using var conn = CreateConnection();
            var user = await conn.QueryFirstOrDefaultAsync(
                "SELECT id, username, role FROM users WHERE username = @Username AND password_hash = @PasswordHash",
                new { dto.Username, PasswordHash = HashPassword(dto.Password) });

            if (user == null) return null;

            return new LoginResponseDto
            {
                Token = GenerateToken((string)user.username, (string)user.role),
                Username = (string)user.username,
                Role = (string)user.role
            };
        }
    }
}
