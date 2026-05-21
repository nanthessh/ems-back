using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;

namespace MyApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollRepository _repo;
        public PayrollController(IPayrollRepository repo) => _repo = repo;

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? month, [FromQuery] int? year)
        {
            var data = await _repo.GetAllAsync(month, year);
            return Ok(ApiResponse<IEnumerable<PayrollDto>>.Ok(data));
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var data = await _repo.GetByEmployeeAsync(employeeId);
            return Ok(ApiResponse<IEnumerable<PayrollDto>>.Ok(data));
        }

        [HttpGet("slip/{id}")]
        public async Task<IActionResult> GetPaySlip(int id)
        {
            var data = await _repo.GetPaySlipAsync(id);
            if (data == null)
                return NotFound(ApiResponse<PayrollDto>.Fail("Pay slip not found."));
            return Ok(ApiResponse<PayrollDto>.Ok(data));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("generate")]
        public async Task<IActionResult> Generate(GeneratePayrollDto dto)
        {
            await _repo.GenerateAsync(dto);
            return Ok(ApiResponse<string>.Ok("Payroll generated successfully."));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus(UpdatePayrollStatusDto dto)
        {
            await _repo.UpdateStatusAsync(dto);
            return Ok(ApiResponse<string>.Ok("Payroll status updated."));
        }
    }
}
