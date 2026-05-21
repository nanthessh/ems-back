using Dapper;
using Microsoft.Data.SqlClient;
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
            => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<IEnumerable<AnnouncementDto>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<AnnouncementDto>(
                "sp_GetAllAnnouncements",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<AnnouncementDto?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<AnnouncementDto>(
                "sp_GetAnnouncementById",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task CreateAsync(AnnouncementDto dto)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(
                "sp_CreateAnnouncement",
                new { dto.Title, dto.Content, dto.Priority, dto.PostedBy, dto.ExpiresOn },
                commandType: CommandType.StoredProcedure);
        }

        public async Task UpdateAsync(AnnouncementDto dto)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(
                "sp_UpdateAnnouncement",
                new { dto.Id, dto.Title, dto.Content, dto.Priority, dto.ExpiresOn },
                commandType: CommandType.StoredProcedure);
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(
                "sp_DeleteAnnouncement",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }
    }
}
