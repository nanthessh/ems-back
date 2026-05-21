using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;
using System.Security.Claims;

namespace MyApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementRepository _repo;
        public AnnouncementsController(IAnnouncementRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _repo.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<AnnouncementDto>>.Ok(data));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _repo.GetByIdAsync(id);
            if (data == null)
                return NotFound(ApiResponse<AnnouncementDto>.Fail("Announcement not found."));
            return Ok(ApiResponse<AnnouncementDto>.Ok(data));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(AnnouncementDto dto)
        {
            dto.PostedBy = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
            await _repo.CreateAsync(dto);
            return Ok(ApiResponse<string>.Ok("Announcement posted successfully."));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update(AnnouncementDto dto)
        {
            await _repo.UpdateAsync(dto);
            return Ok(ApiResponse<string>.Ok("Announcement updated successfully."));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            return Ok(ApiResponse<string>.Ok("Announcement deleted."));
        }
    }
}
