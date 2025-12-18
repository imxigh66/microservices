using IdentityService.Models;

namespace IdentityService.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    }
}
