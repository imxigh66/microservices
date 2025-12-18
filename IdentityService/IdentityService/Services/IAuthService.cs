using IdentityService.DTO;

namespace IdentityService.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAfterTwoFactorAsync(string userId);
    }
}
