using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;

namespace MyApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceRepository _repo;
        public AttendanceController(IAttendanceRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? month, [FromQuery] int? year)
        {
            var data = await _repo.GetAllAsync(month, year);
            return Ok(ApiResponse<IEnumerable<AttendanceDto>>.Ok(data));
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetToday()
        {
            var data = await _repo.GetTodayAsync();
            return Ok(ApiResponse<IEnumerable<AttendanceDto>>.Ok(data));
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployee(int employeeId, [FromQuery] int? month, [FromQuery] int? year)
        {
            var data = await _repo.GetByEmployeeAsync(employeeId, month, year);
            return Ok(ApiResponse<IEnumerable<AttendanceDto>>.Ok(data));
        }

        [HttpGet("summary/{employeeId}")]
        public async Task<IActionResult> GetSummary(int employeeId, [FromQuery] int month, [FromQuery] int year)
        {
            var data = await _repo.GetSummaryAsync(employeeId, month, year);
            return Ok(ApiResponse<AttendanceSummaryDto>.Ok(data));
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn(CheckInDto dto)
        {
            await _repo.CheckInAsync(dto);
            return Ok(ApiResponse<string>.Ok("Checked in successfully."));
        }

        [HttpPut("checkout/{employeeId}")]
        public async Task<IActionResult> CheckOut(int employeeId)
        {
            await _repo.CheckOutAsync(employeeId);
            return Ok(ApiResponse<string>.Ok("Checked out successfully."));
        }
    }
}
