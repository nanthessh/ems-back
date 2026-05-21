using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;

namespace MyApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentRepository _repo;

        public DepartmentsController(IDepartmentRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _repo.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<DepartmentDto>>.Ok(data));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _repo.GetByIdAsync(id);
            if (data == null)
                return NotFound(ApiResponse<DepartmentDto>.Fail("Department not found."));
            return Ok(ApiResponse<DepartmentDto>.Ok(data));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(DepartmentDto dto)
        {
            await _repo.CreateAsync(dto);
            return Ok(ApiResponse<string>.Ok("Department created successfully."));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update(DepartmentDto dto)
        {
            await _repo.UpdateAsync(dto);
            return Ok(ApiResponse<string>.Ok("Department updated successfully."));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            return Ok(ApiResponse<string>.Ok("Department deleted successfully."));
        }
    }
}
