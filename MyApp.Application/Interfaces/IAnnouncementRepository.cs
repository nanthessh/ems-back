using MyApp.Application.DTOs;

namespace MyApp.Application.Interfaces
{
    public interface IAnnouncementRepository
    {
        Task<IEnumerable<AnnouncementDto>> GetAllAsync();
        Task<AnnouncementDto?> GetByIdAsync(int id);
        Task CreateAsync(AnnouncementDto dto);
        Task UpdateAsync(AnnouncementDto dto);
        Task DeleteAsync(int id);
    }
}
