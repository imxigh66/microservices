using IdentityService.Models;

namespace IdentityService.Strategies
{
    public interface ITwoFactorStrategy
    {
        AuthenticationType Type { get; }
        Task<TwoFactorSendResult> SendCodeAsync(TwoFactorContext context);
        Task<bool> VerifyCodeAsync(TwoFactorContext context, string code);
        string GetMaskDestination(TwoFactorContext context);
    }
}
