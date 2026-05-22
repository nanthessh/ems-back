using Dapper;
using Npgsql;
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
            => new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<IEnumerable<AttendanceDto>> GetAllAsync(int? month, int? year)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<AttendanceDto>(@"
                SELECT a.id, a.employee_id AS EmployeeId, e.name AS EmployeeName,
                       d.name AS DepartmentName, a.date, a.check_in AS CheckIn,
                       a.check_out AS CheckOut, a.status, a.notes
                FROM attendance a
                INNER JOIN employees e ON a.employee_id = e.id
                INNER JOIN departments d ON e.department_id = d.id
                WHERE (@Month::int IS NULL OR EXTRACT(MONTH FROM a.date) = @Month)
                  AND (@Year::int  IS NULL OR EXTRACT(YEAR  FROM a.date) = @Year)
                ORDER BY a.date DESC, e.name",
                new { Month = month, Year = year });
        }

        public async Task<IEnumerable<AttendanceDto>> GetByEmployeeAsync(int employeeId, int? month, int? year)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<AttendanceDto>(@"
                SELECT a.id, a.employee_id AS EmployeeId, e.name AS EmployeeName,
                       a.date, a.check_in AS CheckIn, a.check_out AS CheckOut, a.status, a.notes
                FROM attendance a
                INNER JOIN employees e ON a.employee_id = e.id
                WHERE a.employee_id = @EmployeeId
                  AND (@Month::int IS NULL OR EXTRACT(MONTH FROM a.date) = @Month)
                  AND (@Year::int  IS NULL OR EXTRACT(YEAR  FROM a.date) = @Year)
                ORDER BY a.date DESC",
                new { EmployeeId = employeeId, Month = month, Year = year });
        }

        public async Task<IEnumerable<AttendanceDto>> GetTodayAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<AttendanceDto>(@"
                SELECT a.id, a.employee_id AS EmployeeId, e.name AS EmployeeName,
                       d.name AS DepartmentName, a.date, a.check_in AS CheckIn,
                       a.check_out AS CheckOut, a.status, a.notes
                FROM attendance a
                INNER JOIN employees e ON a.employee_id = e.id
                INNER JOIN departments d ON e.department_id = d.id
                WHERE a.date = CURRENT_DATE
                ORDER BY a.check_in");
        }

        public async Task CheckInAsync(CheckInDto dto)
        {
            using var conn = CreateConnection();
            var now = TimeOnly.FromDateTime(DateTime.Now);
            var status = now > new TimeOnly(9, 30) ? "Late" : "Present";

            await conn.ExecuteAsync(@"
                INSERT INTO attendance (employee_id, date, check_in, status, notes)
                VALUES (@EmployeeId, CURRENT_DATE, @Now, @Status, @Notes)
                ON CONFLICT (employee_id, date)
                DO UPDATE SET check_in = @Now, status = @Status, notes = @Notes",
                new { dto.EmployeeId, Now = now, Status = status, dto.Notes });
        }

        public async Task CheckOutAsync(int employeeId)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(@"
                UPDATE attendance SET check_out = @Now
                WHERE employee_id = @EmployeeId AND date = CURRENT_DATE",
                new { EmployeeId = employeeId, Now = TimeOnly.FromDateTime(DateTime.Now) });
        }

        public async Task<AttendanceSummaryDto> GetSummaryAsync(int employeeId, int month, int year)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstAsync<AttendanceSummaryDto>(@"
                SELECT
                    COUNT(*)                                                  AS TotalDays,
                    SUM(CASE WHEN status = 'Present'  THEN 1 ELSE 0 END)     AS PresentDays,
                    SUM(CASE WHEN status = 'Absent'   THEN 1 ELSE 0 END)     AS AbsentDays,
                    SUM(CASE WHEN status = 'Late'     THEN 1 ELSE 0 END)     AS LateDays,
                    SUM(CASE WHEN status = 'Half-Day' THEN 1 ELSE 0 END)     AS HalfDays
                FROM attendance
                WHERE employee_id = @EmployeeId
                  AND EXTRACT(MONTH FROM date) = @Month
                  AND EXTRACT(YEAR  FROM date) = @Year",
                new { EmployeeId = employeeId, Month = month, Year = year });
        }
    }
}
