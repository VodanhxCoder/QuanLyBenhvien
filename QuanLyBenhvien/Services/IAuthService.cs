using HospitalFrontend.Models;

namespace Fontend.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> ExternalLoginAsync(ExternalLoginDto loginDto);
        Task RegisterAsync(RegisterDto registerDto);
        Task<UserDto> GetUserByEmailAsync(string email); // Thêm phương thức này
    }
}
