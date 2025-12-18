namespace IdentityService.DTO
{
    public class TwoFactorVerifyRequest
    {
        public string Token { get; set; } 
        public string Code { get; set; }
    }
}
