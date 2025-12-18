using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UserService.Services
{
    public class ProfileService:IProfileService
    {
        private readonly UserDbContext _dbContext;
        public ProfileService(UserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProfileResponse?> GetProfileAsync(string userId)
        {
            var userProfile = await _dbContext.UserProfiles
                .Include(p=>p.WishlistItems)
                .Include(p=>p.CartItems)
                .FirstOrDefaultAsync(p=>p.UserId==userId);

            if (userProfile == null)
            {
                return null;
            }

            return new ProfileResponse
            {
                UserId = userProfile.UserId,
                Email = userProfile.Email,
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                PhoneNumber = userProfile.PhoneNumber,
                Country = userProfile.Country,
                City = userProfile.City,
                CreatedAt = userProfile.CreatedAt,
                WishlistCount = userProfile.WishlistItems.Count,
                CartItemsCount = userProfile.CartItems.Count
            };
        }

        public async Task<ProfileResponse?> UpdateProfileAsync(string userId, UpdateProfileRequest request)
        {
            var profile = await _dbContext.UserProfiles.FindAsync(userId);

            if (profile == null)
                return null;

            profile.FirstName = request.FirstName ?? profile.FirstName;
            profile.LastName = request.LastName ?? profile.LastName;
            profile.PhoneNumber = request.PhoneNumber ?? profile.PhoneNumber;
            profile.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return await GetProfileAsync(userId);
        }
    }
}
