using MyApp.Application.DTOs;

namespace MyApp.Application.Interfaces
{
    public interface IAuthRepository
    {
        Task<int> RegisterAsync(RegisterDto dto);
        Task<LoginResponseDto?> LoginAsync(LoginDto dto);
    }
}
