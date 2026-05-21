using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;

namespace MyApp.Infrastructure.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly IConfiguration _config;

        public DepartmentRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
            => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<IEnumerable<DepartmentDto>> GetAllAsync()
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<DepartmentDto>(
                "sp_GetAllDepartments",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<DepartmentDto?> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<DepartmentDto>(
                "sp_GetDepartmentById",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CreateAsync(DepartmentDto department)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync(
                "sp_InsertDepartment",
                new { department.Name, department.Description },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateAsync(DepartmentDto department)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync(
                "sp_UpdateDepartment",
                new { department.Id, department.Name, department.Description },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync(
                "sp_DeleteDepartment",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentIdAsync(int departmentId)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<EmployeeDto>(
                "sp_GetEmployeesByDepartmentId",
                new { DepartmentId = departmentId },
                commandType: CommandType.StoredProcedure);
        }
    }
}
