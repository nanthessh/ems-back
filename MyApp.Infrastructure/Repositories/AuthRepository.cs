using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
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

        public AuthRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
            => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

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
            using var connection = CreateConnection();
            return await connection.ExecuteAsync(
                "sp_RegisterUser",
                new { dto.Username, PasswordHash = HashPassword(dto.Password), dto.Role },
                commandType: CommandType.StoredProcedure
                );
        }
        public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
        {
            using var connection = CreateConnection();
            var user = await connection.QueryFirstOrDefaultAsync(
                "sp_LoginUser",
                new { dto.Username, PasswordHash = HashPassword(dto.Password) },
                commandType: CommandType.StoredProcedure);

            if (user == null) return null;

            return new LoginResponseDto
            {
                Token = GenerateToken((string)user.Username, (string)user.Role),
                Username = (string)user.Username,
                Role = (string)user.Role
            };
        }
    }
}
