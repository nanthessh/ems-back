using MyApp.Application.DTOs;

namespace MyApp.Application.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<EmployeeDto>> GetAllAsync();
        Task<EmployeeDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(EmployeeDto employee);
        Task<int> UpdateAsync(EmployeeDto employee);
        Task<int> DeleteAsync(int id);
        Task<DashboardStatsDto> GetDashboardStatsAsync();
    }
}
