namespace IdentityService.Strategies
{
    public class TwoFactorSendResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string MaskedDestination { get; set; }
        public string? Token { get; set; }
    }
}
