using IdentityService.Models;

namespace IdentityService.DTO
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public UserInfo? User { get; set; }

        public bool RequiresTwoFactor { get; set; }
        public string TwoFactorToken { get; set; } // Временный токен для 2FA
        public AuthenticationType? Type { get; set; }
        public string MaskedDestination { get; set; } // Например: "***@gmail.com"
    }
}
