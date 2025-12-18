using IdentityService.Models;
using IdentityService.Strategies;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace IdentityService.Services
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly IEnumerable<ITwoFactorStrategy> _strategies;
        private readonly ILogger<TwoFactorService> _logger;
        private readonly IMemoryCache _cache;

        public TwoFactorService(IEnumerable<ITwoFactorStrategy> strategies,ILogger<TwoFactorService> logger,IMemoryCache cache)
        {
            _cache = cache;
            _strategies = strategies;
            _logger = logger;
        }
        public TwoFactorContext GetContext(string token)
        {
            if(string.IsNullOrEmpty(token))
            {
                return null;
            }
            _cache.TryGetValue<TwoFactorContext>($"2fa:context:{token}", out var context);
            return context;
        }

        public async Task<TwoFactorSendResult> InitialTwoFactorAsync(ApplicationUser user, AuthenticationType type)
        {
            var stretegy = _strategies.FirstOrDefault(s => s.Type == type);
            if(stretegy == null)
            {
                _logger.LogError($"No 2FA strategy found for type {type} for user {user.Id}", user.Id);
                return new TwoFactorSendResult
                {
                    Success = false,
                    Message = "Invalid 2FA method"
                };
            }

            var context= new TwoFactorContext
            {
                UserId = user.Id,
                User = user,
                Email = user.Email,
                Phone = user.PhoneNumber,
                Type = type,
                Token = GenerateToken(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            _logger.LogInformation("Saving 2FA context: key=2fa:context:{Token}", context.Token);

            _cache.Set($"2fa:context:{context.Token}", context, TimeSpan.FromMinutes(10));

            var result = await stretegy.SendCodeAsync(context);
            if (result.Success)
            {
                result.MaskedDestination = stretegy.GetMaskDestination(context);
                result.Token = context.Token;
            }
            return result;


            }

        private string GenerateToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public async Task<bool> VerifyTwoFactorAsync(string token, string code)
        {
            _logger.LogInformation("Verifying 2FA: token={Token}, code={Code}", token, code);
            var context = GetContext(token);
            _logger.LogInformation("Context found: {Found}", context != null);
            if (context == null)
            {
                _logger.LogWarning($"2FA context not found or expired for token {token}");
                return false;
            }

            var stretegy = _strategies.FirstOrDefault(s => s.Type == context.Type);
            if (stretegy == null)
            {
                _logger.LogError($"No 2FA strategy found for type {context.Type} for user {context.UserId}", context.UserId);
                return false;
            }

            var isValid = await stretegy.VerifyCodeAsync(context, code);
            if (isValid)
            {
                _cache.Remove($"2fa:context:{token}");
            }

            return isValid;
        }
    }
}
