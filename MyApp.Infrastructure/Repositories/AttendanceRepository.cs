using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;
using System.Data;

namespace MyApp.Infrastructure.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly IConfiguration _config;
        public AttendanceRepository(IConfiguration config) => _config = config;

        private IDbConnection CreateConnection()
            => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<IEnumerable<AttendanceDto>> GetAllAsync(int? month, int? year)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<AttendanceDto>(
                "sp_GetAllAttendance",
                new { Month = month, Year = year },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<AttendanceDto>> GetByEmployeeAsync(int employeeId, int? month, int? year)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<AttendanceDto>(
                "sp_GetAttendanceByEmployee",
                new { EmployeeId = employeeId, Month = month, Year = year },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<AttendanceDto>> GetTodayAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<AttendanceDto>(
                "sp_GetTodayAttendance",
                commandType: CommandType.StoredProcedure);
        }

        public async Task CheckInAsync(CheckInDto dto)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(
                "sp_CheckIn",
                new { dto.EmployeeId, dto.Notes },
                commandType: CommandType.StoredProcedure);
        }

        public async Task CheckOutAsync(int employeeId)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(
                "sp_CheckOut",
                new { EmployeeId = employeeId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<AttendanceSummaryDto> GetSummaryAsync(int employeeId, int month, int year)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstAsync<AttendanceSummaryDto>(
                "sp_GetAttendanceSummary",
                new { EmployeeId = employeeId, Month = month, Year = year },
                commandType: CommandType.StoredProcedure);
        }
    }
}
