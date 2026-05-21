using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;

namespace MyApp.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IConfiguration _config;

        public EmployeeRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
            => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<EmployeeDto>(
                "sp_GetAllEmployees",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<EmployeeDto?> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<EmployeeDto>(
                "sp_GetEmployeeById",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CreateAsync(EmployeeDto employee)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync(
                "sp_InsertEmployee",
                new { employee.Name, employee.Email, employee.DepartmentId, employee.Salary, employee.JoinedDate },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateAsync(EmployeeDto employee)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync(
                "sp_UpdateEmployee",
                new { employee.Id, employee.Name, employee.Email, employee.DepartmentId, employee.Salary, employee.JoinedDate },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync(
                "sp_DeleteEmployee",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstAsync<DashboardStatsDto>(
                "sp_GetDashboardStats",
                commandType: CommandType.StoredProcedure);
        }
    }
}
