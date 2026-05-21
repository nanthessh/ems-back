using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;

namespace MyApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveRepository _repo;
        public LeaveController(ILeaveRepository repo) => _repo = repo;

        [HttpGet("types")]
        public async Task<IActionResult> GetLeaveTypes()
        {
            var data = await _repo.GetLeaveTypesAsync();
            return Ok(ApiResponse<IEnumerable<LeaveTypeDto>>.Ok(data));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var data = await _repo.GetAllAsync(status);
            return Ok(ApiResponse<IEnumerable<LeaveRequestDto>>.Ok(data));
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var data = await _repo.GetByEmployeeAsync(employeeId);
            return Ok(ApiResponse<IEnumerable<LeaveRequestDto>>.Ok(data));
        }

        [HttpGet("balance/{employeeId}")]
        public async Task<IActionResult> GetBalance(int employeeId, [FromQuery] int year)
        {
            var data = await _repo.GetBalanceAsync(employeeId, year);
            return Ok(ApiResponse<IEnumerable<LeaveBalanceDto>>.Ok(data));
        }

        [HttpPost("apply")]
        public async Task<IActionResult> Apply(ApplyLeaveDto dto)
        {
            await _repo.ApplyAsync(dto);
            return Ok(ApiResponse<string>.Ok("Leave request submitted successfully."));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus(UpdateLeaveStatusDto dto)
        {
            await _repo.UpdateStatusAsync(dto);
            return Ok(ApiResponse<string>.Ok($"Leave request {dto.Status.ToLower()} successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            return Ok(ApiResponse<string>.Ok("Leave request cancelled."));
        }
    }
}
