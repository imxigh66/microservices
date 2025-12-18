namespace IdentityService.Models
{
    public class TwoFactorContext
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public AuthenticationType Type { get; set; }
        public string Code { get; set; }
        public string Token { get; set; } // Временный токен для верификации
        public DateTime ExpiresAt { get; set; }
        public int AttemptsRemaining { get; set; } = 3;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
