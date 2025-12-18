using Microsoft.AspNetCore.Identity;

namespace IdentityService.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        //public bool TwoFactorEnabled { get; set; } = false;
        public AuthenticationType? PreferredTwoFactorMethod { get; set; }
    }
}
