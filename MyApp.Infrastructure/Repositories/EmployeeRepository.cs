using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using System.Data;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;

namespace MyApp.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IConfiguration _config;

        public EmployeeRepository(IConfiguration config) => _config = config;

        private IDbConnection CreateConnection()
            => new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<EmployeeDto>(@"
                SELECT e.id, e.name, e.email, e.department_id AS DepartmentId,
                       d.name AS DepartmentName, e.salary, e.joined_date AS JoinedDate
                FROM employees e
                INNER JOIN departments d ON e.department_id = d.id
                ORDER BY e.id DESC");
        }

        public async Task<EmployeeDto?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<EmployeeDto>(@"
                SELECT e.id, e.name, e.email, e.department_id AS DepartmentId,
                       d.name AS DepartmentName, e.salary, e.joined_date AS JoinedDate
                FROM employees e
                INNER JOIN departments d ON e.department_id = d.id
                WHERE e.id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(EmployeeDto employee)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(@"
                INSERT INTO employees (name, email, department_id, salary, joined_date)
                VALUES (@Name, @Email, @DepartmentId, @Salary, @JoinedDate)",
                new { employee.Name, employee.Email, employee.DepartmentId, employee.Salary, employee.JoinedDate });
        }

        public async Task<int> UpdateAsync(EmployeeDto employee)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(@"
                UPDATE employees SET name=@Name, email=@Email, department_id=@DepartmentId,
                       salary=@Salary, joined_date=@JoinedDate
                WHERE id = @Id",
                new { employee.Id, employee.Name, employee.Email, employee.DepartmentId, employee.Salary, employee.JoinedDate });
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync("DELETE FROM employees WHERE id = @Id", new { Id = id });
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstAsync<DashboardStatsDto>(@"
                SELECT
                    (SELECT COUNT(*) FROM employees)    AS TotalEmployees,
                    (SELECT COUNT(*) FROM departments)  AS TotalDepartments,
                    (SELECT COALESCE(SUM(salary), 0) FROM employees) AS TotalSalary,
                    (SELECT COUNT(*) FROM employees
                     WHERE EXTRACT(MONTH FROM joined_date) = EXTRACT(MONTH FROM NOW())
                       AND EXTRACT(YEAR  FROM joined_date) = EXTRACT(YEAR  FROM NOW())) AS NewEmployeesThisMonth");
        }
    }
}
