using MyApp.Application.DTOs;

namespace MyApp.Application.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<IEnumerable<AttendanceDto>> GetAllAsync(int? month, int? year);
        Task<IEnumerable<AttendanceDto>> GetByEmployeeAsync(int employeeId, int? month, int? year);
        Task<IEnumerable<AttendanceDto>> GetTodayAsync();
        Task CheckInAsync(CheckInDto dto);
        Task CheckOutAsync(int employeeId);
        Task<AttendanceSummaryDto> GetSummaryAsync(int employeeId, int month, int year);
    }
}
