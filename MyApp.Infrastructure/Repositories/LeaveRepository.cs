using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;
using System.Data;

namespace MyApp.Infrastructure.Repositories
{
    public class LeaveRepository : ILeaveRepository
    {
        private readonly IConfiguration _config;
        public LeaveRepository(IConfiguration config) => _config = config;

        private IDbConnection CreateConnection()
            => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<IEnumerable<LeaveTypeDto>> GetLeaveTypesAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<LeaveTypeDto>(
                "sp_GetLeaveTypes",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetAllAsync(string? status)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<LeaveRequestDto>(
                "sp_GetAllLeaveRequests",
                new { Status = status },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetByEmployeeAsync(int employeeId)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<LeaveRequestDto>(
                "sp_GetLeaveByEmployee",
                new { EmployeeId = employeeId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task ApplyAsync(ApplyLeaveDto dto)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(
                "sp_ApplyLeave",
                new { dto.EmployeeId, dto.LeaveTypeId, dto.StartDate, dto.EndDate, dto.Reason },
                commandType: CommandType.StoredProcedure);
        }

        public async Task UpdateStatusAsync(UpdateLeaveStatusDto dto)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(
                "sp_UpdateLeaveStatus",
                new { dto.Id, dto.Status, dto.AdminNote },
                commandType: CommandType.StoredProcedure);
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(
                "sp_DeleteLeaveRequest",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<LeaveBalanceDto>> GetBalanceAsync(int employeeId, int year)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<LeaveBalanceDto>(
                "sp_GetLeaveBalance",
                new { EmployeeId = employeeId, Year = year },
                commandType: CommandType.StoredProcedure);
        }
    }
}
