using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;
using System.Data;

namespace MyApp.Infrastructure.Repositories
{
    public class PayrollRepository : IPayrollRepository
    {
        private readonly IConfiguration _config;
        public PayrollRepository(IConfiguration config) => _config = config;

        private IDbConnection CreateConnection()
            => new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<IEnumerable<PayrollDto>> GetAllAsync(int? month, int? year)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<PayrollDto>(@"
                SELECT p.id, p.employee_id AS EmployeeId, e.name AS EmployeeName,
                       d.name AS DepartmentName, p.month, p.year,
                       p.basic_salary AS BasicSalary, p.house_allowance AS HouseAllowance,
                       p.transport_allow AS TransportAllow, p.overtime_pay AS OvertimePay,
                       p.tax_deduction AS TaxDeduction, p.other_deductions AS OtherDeductions,
                       p.net_salary AS NetSalary, p.status, p.paid_on AS PaidOn, p.generated_on AS GeneratedOn
                FROM payroll p
                INNER JOIN employees   e ON p.employee_id   = e.id
                INNER JOIN departments d ON e.department_id = d.id
                WHERE (@Month::int IS NULL OR p.month = @Month)
                  AND (@Year::int  IS NULL OR p.year  = @Year)
                ORDER BY p.year DESC, p.month DESC, e.name",
                new { Month = month, Year = year });
        }

        public async Task<IEnumerable<PayrollDto>> GetByEmployeeAsync(int employeeId)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<PayrollDto>(@"
                SELECT p.id, p.employee_id AS EmployeeId, e.name AS EmployeeName,
                       p.month, p.year,
                       p.basic_salary AS BasicSalary, p.house_allowance AS HouseAllowance,
                       p.transport_allow AS TransportAllow, p.overtime_pay AS OvertimePay,
                       p.tax_deduction AS TaxDeduction, p.other_deductions AS OtherDeductions,
                       p.net_salary AS NetSalary, p.status, p.paid_on AS PaidOn, p.generated_on AS GeneratedOn
                FROM payroll p
                INNER JOIN employees e ON p.employee_id = e.id
                WHERE p.employee_id = @EmployeeId
                ORDER BY p.year DESC, p.month DESC",
                new { EmployeeId = employeeId });
        }

        public async Task<PayrollDto?> GetPaySlipAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<PayrollDto>(@"
                SELECT p.id, p.employee_id AS EmployeeId, e.name AS EmployeeName,
                       e.email, d.name AS DepartmentName, p.month, p.year,
                       p.basic_salary AS BasicSalary, p.house_allowance AS HouseAllowance,
                       p.transport_allow AS TransportAllow, p.overtime_pay AS OvertimePay,
                       p.tax_deduction AS TaxDeduction, p.other_deductions AS OtherDeductions,
                       p.net_salary AS NetSalary, p.status, p.paid_on AS PaidOn, p.generated_on AS GeneratedOn
                FROM payroll p
                INNER JOIN employees   e ON p.employee_id   = e.id
                INNER JOIN departments d ON e.department_id = d.id
                WHERE p.id = @Id", new { Id = id });
        }

        public async Task GenerateAsync(GeneratePayrollDto dto)
        {
            using var conn = CreateConnection();
            var basicSalary = await conn.ExecuteScalarAsync<decimal>(
                "SELECT salary FROM employees WHERE id = @EmployeeId", new { dto.EmployeeId });

            var netSalary = basicSalary + dto.HouseAllowance + dto.TransportAllow + dto.OvertimePay
                            - dto.TaxDeduction - dto.OtherDeductions;

            await conn.ExecuteAsync(@"
                INSERT INTO payroll (employee_id, month, year, basic_salary, house_allowance, transport_allow,
                                     overtime_pay, tax_deduction, other_deductions, net_salary)
                VALUES (@EmployeeId, @Month, @Year, @BasicSalary, @HouseAllowance, @TransportAllow,
                        @OvertimePay, @TaxDeduction, @OtherDeductions, @NetSalary)
                ON CONFLICT (employee_id, month, year)
                DO UPDATE SET basic_salary=@BasicSalary, house_allowance=@HouseAllowance,
                              transport_allow=@TransportAllow, overtime_pay=@OvertimePay,
                              tax_deduction=@TaxDeduction, other_deductions=@OtherDeductions,
                              net_salary=@NetSalary, generated_on=NOW()",
                new { dto.EmployeeId, dto.Month, dto.Year, BasicSalary = basicSalary, dto.HouseAllowance,
                      dto.TransportAllow, dto.OvertimePay, dto.TaxDeduction, dto.OtherDeductions, NetSalary = netSalary });
        }

        public async Task UpdateStatusAsync(UpdatePayrollStatusDto dto)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(@"
                UPDATE payroll SET status=@Status,
                       paid_on = CASE WHEN @Status = 'Paid' THEN NOW() ELSE NULL END
                WHERE id = @Id",
                new { dto.Id, dto.Status });
        }
    }
}
