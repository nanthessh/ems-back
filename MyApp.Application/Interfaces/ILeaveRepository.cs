using MyApp.Application.DTOs;

namespace MyApp.Application.Interfaces
{
    public interface ILeaveRepository
    {
        Task<IEnumerable<LeaveTypeDto>> GetLeaveTypesAsync();
        Task<IEnumerable<LeaveRequestDto>> GetAllAsync(string? status);
        Task<IEnumerable<LeaveRequestDto>> GetByEmployeeAsync(int employeeId);
        Task ApplyAsync(ApplyLeaveDto dto);
        Task UpdateStatusAsync(UpdateLeaveStatusDto dto);
        Task DeleteAsync(int id);
        Task<IEnumerable<LeaveBalanceDto>> GetBalanceAsync(int employeeId, int year);
    }
}
