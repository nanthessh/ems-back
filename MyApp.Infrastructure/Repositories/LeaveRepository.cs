using Dapper;
using Npgsql;
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
            => new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<IEnumerable<LeaveTypeDto>> GetLeaveTypesAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<LeaveTypeDto>(
                "SELECT id, name, max_days AS MaxDays FROM leave_types ORDER BY name");
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetAllAsync(string? status)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<LeaveRequestDto>(@"
                SELECT lr.id, lr.employee_id AS EmployeeId, e.name AS EmployeeName,
                       d.name AS DepartmentName, lt.name AS LeaveTypeName,
                       lr.start_date AS StartDate, lr.end_date AS EndDate, lr.total_days AS TotalDays,
                       lr.reason, lr.status, lr.admin_note AS AdminNote,
                       lr.applied_on AS AppliedOn, lr.reviewed_on AS ReviewedOn
                FROM leave_requests lr
                INNER JOIN employees  e  ON lr.employee_id  = e.id
                INNER JOIN departments d  ON e.department_id = d.id
                INNER JOIN leave_types lt ON lr.leave_type_id = lt.id
                WHERE @Status::text IS NULL OR lr.status = @Status
                ORDER BY lr.applied_on DESC",
                new { Status = status });
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetByEmployeeAsync(int employeeId)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<LeaveRequestDto>(@"
                SELECT lr.id, lr.employee_id AS EmployeeId, e.name AS EmployeeName,
                       lt.name AS LeaveTypeName, lr.leave_type_id AS LeaveTypeId,
                       lr.start_date AS StartDate, lr.end_date AS EndDate, lr.total_days AS TotalDays,
                       lr.reason, lr.status, lr.admin_note AS AdminNote,
                       lr.applied_on AS AppliedOn, lr.reviewed_on AS ReviewedOn
                FROM leave_requests lr
                INNER JOIN employees  e  ON lr.employee_id  = e.id
                INNER JOIN leave_types lt ON lr.leave_type_id = lt.id
                WHERE lr.employee_id = @EmployeeId
                ORDER BY lr.applied_on DESC",
                new { EmployeeId = employeeId });
        }

        public async Task ApplyAsync(ApplyLeaveDto dto)
        {
            using var conn = CreateConnection();
            var totalDays = (dto.EndDate - dto.StartDate).Days + 1;
            await conn.ExecuteAsync(@"
                INSERT INTO leave_requests (employee_id, leave_type_id, start_date, end_date, total_days, reason)
                VALUES (@EmployeeId, @LeaveTypeId, @StartDate, @EndDate, @TotalDays, @Reason)",
                new { dto.EmployeeId, dto.LeaveTypeId, dto.StartDate, dto.EndDate, TotalDays = totalDays, dto.Reason });
        }

        public async Task UpdateStatusAsync(UpdateLeaveStatusDto dto)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(@"
                UPDATE leave_requests SET status=@Status, admin_note=@AdminNote, reviewed_on=NOW()
                WHERE id = @Id",
                new { dto.Id, dto.Status, dto.AdminNote });
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(
                "DELETE FROM leave_requests WHERE id = @Id AND status = 'Pending'",
                new { Id = id });
        }

        public async Task<IEnumerable<LeaveBalanceDto>> GetBalanceAsync(int employeeId, int year)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<LeaveBalanceDto>(@"
                SELECT lt.id AS LeaveTypeId, lt.name AS LeaveTypeName, lt.max_days AS MaxDays,
                       COALESCE(SUM(CASE WHEN lr.status = 'Approved' THEN lr.total_days ELSE 0 END), 0) AS UsedDays,
                       lt.max_days - COALESCE(SUM(CASE WHEN lr.status = 'Approved' THEN lr.total_days ELSE 0 END), 0) AS RemainingDays
                FROM leave_types lt
                LEFT JOIN leave_requests lr
                    ON lt.id = lr.leave_type_id
                    AND lr.employee_id = @EmployeeId
                    AND EXTRACT(YEAR FROM lr.start_date) = @Year
                GROUP BY lt.id, lt.name, lt.max_days
                ORDER BY lt.name",
                new { EmployeeId = employeeId, Year = year });
        }
    }
}
