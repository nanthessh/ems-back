using MyApp.Application.DTOs;

namespace MyApp.Application.Interfaces
{
    public interface IPayrollRepository
    {
        Task<IEnumerable<PayrollDto>> GetAllAsync(int? month, int? year);
        Task<IEnumerable<PayrollDto>> GetByEmployeeAsync(int employeeId);
        Task<PayrollDto?> GetPaySlipAsync(int id);
        Task GenerateAsync(GeneratePayrollDto dto);
        Task UpdateStatusAsync(UpdatePayrollStatusDto dto);
    }
}
