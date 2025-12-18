using IdentityService.Models;
using IdentityService.Strategies;

namespace IdentityService.Services
{
    public interface ITwoFactorService
    {
        Task<TwoFactorSendResult> InitialTwoFactorAsync(ApplicationUser user,AuthenticationType type);
        Task<bool> VerifyTwoFactorAsync(string token, string code);
        TwoFactorContext GetContext(string token);
    }
}
