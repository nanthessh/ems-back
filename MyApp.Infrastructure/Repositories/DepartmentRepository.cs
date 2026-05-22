using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using System.Data;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;

namespace MyApp.Infrastructure.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly IConfiguration _config;

        public DepartmentRepository(IConfiguration config) => _config = config;

        private IDbConnection CreateConnection()
            => new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<IEnumerable<DepartmentDto>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<DepartmentDto>(
                "SELECT id, name, description FROM departments ORDER BY name");
        }

        public async Task<DepartmentDto?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<DepartmentDto>(
                "SELECT id, name, description FROM departments WHERE id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(DepartmentDto department)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "INSERT INTO departments (name, description) VALUES (@Name, @Description)",
                new { department.Name, department.Description });
        }

        public async Task<int> UpdateAsync(DepartmentDto department)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "UPDATE departments SET name=@Name, description=@Description WHERE id=@Id",
                new { department.Id, department.Name, department.Description });
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM departments WHERE id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentIdAsync(int departmentId)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<EmployeeDto>(@"
                SELECT e.id, e.name, e.email, e.department_id AS DepartmentId,
                       d.name AS DepartmentName, e.salary, e.joined_date AS JoinedDate
                FROM employees e
                INNER JOIN departments d ON e.department_id = d.id
                WHERE e.department_id = @DepartmentId",
                new { DepartmentId = departmentId });
        }
    }
}
