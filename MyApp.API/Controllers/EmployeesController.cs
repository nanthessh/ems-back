using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;

namespace MyApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository _repo;

        public EmployeesController(IEmployeeRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _repo.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<EmployeeDto>>.Ok(data));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _repo.GetByIdAsync(id);
            if (data == null)
                return NotFound(ApiResponse<EmployeeDto>.Fail("Employee not found."));
            return Ok(ApiResponse<EmployeeDto>.Ok(data));
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var data = await _repo.GetDashboardStatsAsync();
            return Ok(ApiResponse<DashboardStatsDto>.Ok(data));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(EmployeeDto dto)
        {
            await _repo.CreateAsync(dto);
            return Ok(ApiResponse<string>.Ok("Employee created successfully."));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update(EmployeeDto dto)
        {
            await _repo.UpdateAsync(dto);
            return Ok(ApiResponse<string>.Ok("Employee updated successfully."));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            return Ok(ApiResponse<string>.Ok("Employee deleted successfully."));
        }
    }
}
