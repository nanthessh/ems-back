using MyApp.Application.DTOs;

namespace MyApp.Application.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<DepartmentDto>> GetAllAsync();
        Task<DepartmentDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(DepartmentDto department);
        Task<int> UpdateAsync(DepartmentDto department);
        Task<int> DeleteAsync(int id);
        Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentIdAsync(int departmentId);
    }
}
