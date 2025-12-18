using UserService.DTOs;

namespace UserService.Services
{
    public interface IProfileService
    {
        Task<ProfileResponse?> GetProfileAsync(string userId);
        Task<ProfileResponse?> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    }
}
