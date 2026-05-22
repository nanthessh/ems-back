using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;
using System.Data;

namespace MyApp.Infrastructure.Repositories
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly IConfiguration _config;
        public AnnouncementRepository(IConfiguration config) => _config = config;

        private IDbConnection CreateConnection()
            => new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<IEnumerable<AnnouncementDto>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<AnnouncementDto>(@"
                SELECT id, title, content, priority, posted_by AS PostedBy,
                       posted_on AS PostedOn, expires_on AS ExpiresOn
                FROM announcements
                WHERE expires_on IS NULL OR expires_on >= CURRENT_DATE
                ORDER BY posted_on DESC");
        }

        public async Task<AnnouncementDto?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<AnnouncementDto>(@"
                SELECT id, title, content, priority, posted_by AS PostedBy,
                       posted_on AS PostedOn, expires_on AS ExpiresOn
                FROM announcements WHERE id = @Id", new { Id = id });
        }

        public async Task CreateAsync(AnnouncementDto dto)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(@"
                INSERT INTO announcements (title, content, priority, posted_by, expires_on)
                VALUES (@Title, @Content, @Priority, @PostedBy, @ExpiresOn)",
                new { dto.Title, dto.Content, dto.Priority, dto.PostedBy, dto.ExpiresOn });
        }

        public async Task UpdateAsync(AnnouncementDto dto)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(@"
                UPDATE announcements SET title=@Title, content=@Content,
                       priority=@Priority, expires_on=@ExpiresOn
                WHERE id = @Id",
                new { dto.Id, dto.Title, dto.Content, dto.Priority, dto.ExpiresOn });
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("DELETE FROM announcements WHERE id = @Id", new { Id = id });
        }
    }
}
