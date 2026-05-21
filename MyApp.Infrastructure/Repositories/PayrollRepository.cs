using Dapper;
using Microsoft.Data.SqlClient;
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
            => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<IEnumerable<PayrollDto>> GetAllAsync(int? month, int? year)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<PayrollDto>(
                "sp_GetAllPayroll",
                new { Month = month, Year = year },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<PayrollDto>> GetByEmployeeAsync(int employeeId)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<PayrollDto>(
                "sp_GetPayrollByEmployee",
                new { EmployeeId = employeeId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<PayrollDto?> GetPaySlipAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<PayrollDto>(
                "sp_GetPaySlip",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task GenerateAsync(GeneratePayrollDto dto)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(
                "sp_GeneratePayroll",
                new
                {
                    dto.EmployeeId, dto.Month, dto.Year,
                    dto.HouseAllowance, dto.TransportAllow, dto.OvertimePay,
                    dto.TaxDeduction, dto.OtherDeductions
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task UpdateStatusAsync(UpdatePayrollStatusDto dto)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(
                "sp_UpdatePayrollStatus",
                new { dto.Id, dto.Status },
                commandType: CommandType.StoredProcedure);
        }
    }
}
