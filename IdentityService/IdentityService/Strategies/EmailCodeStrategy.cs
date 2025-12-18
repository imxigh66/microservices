using IdentityService.Models;
using IdentityService.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace IdentityService.Strategies
{
    public class EmailCodeStrategy : ITwoFactorStrategy
    {
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<EmailCodeStrategy> _logger;
        public AuthenticationType Type => AuthenticationType.EmailCode;

        public EmailCodeStrategy(IEmailService emailSErvice,IMemoryCache cache,ILogger<EmailCodeStrategy> logger)
        {
            _cache = cache;
            _emailService = emailSErvice;
            _logger = logger;
        }
        public string GetMaskDestination(TwoFactorContext context)
        {
            return GetMaskedEmail(context.Email);
        }

        public async Task<TwoFactorSendResult> SendCodeAsync(TwoFactorContext context)
        {
            try
            {
                // Генерируем и сохраняем код прямо в контексте
                context.Code = GenerateSecureCode();
                context.AttemptsRemaining = 3;

                // Обновляем контекст в кеше
                _cache.Set($"2fa:context:{context.Token}", context, TimeSpan.FromMinutes(10));

                // Отправляем email
                await SendEmailAsync(context.Email, context.Code, context.User.FirstName);

                _logger.LogInformation($"2FA code sent to email {context.Email} for user {context.UserId}",GetMaskedEmail(context.Email),context.UserId);

                return new TwoFactorSendResult
                {

                    Success = true,
                    Message = "2FA code sent via email",
                    MaskedDestination = GetMaskedEmail(context.Email)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send 2FA code via email for user {context.UserId}", context.UserId);
                return new TwoFactorSendResult
                {
                    Success = false,
                    Message = "Failed to send 2FA code via email"
                };
            }
        }

        public async Task<bool> VerifyCodeAsync(TwoFactorContext context, string code)
        {
            var cacheKey= $"2fa:context:{context.Token}";

            if (!_cache.TryGetValue<TwoFactorContext>(cacheKey, out var cachedContext))
            {
                _logger.LogWarning($"2FA code expired or not found for user {context.UserId}", context.UserId);
                return false;
            }

            if(cachedContext.AttemptsRemaining <= 0)
            {
                _logger.LogWarning($"No attempts remaining for user {context.UserId}", context.UserId);
                _cache.Remove(cacheKey);
                return false;
            }

            cachedContext.AttemptsRemaining--;

            if(cachedContext.Code != code)
            {
                _cache.Set(cacheKey, cachedContext, TimeSpan.FromMinutes(10));
                _logger.LogWarning("Invalid 2FA code attempt for user {UserId}. Attempts remaining: {Attempts}",
                    context.UserId, cachedContext.AttemptsRemaining);
                return false;
            }
            else
            {
                _cache.Remove(cacheKey);
                _logger.LogInformation("2FA code verified successfully for user {UserId}", context.UserId);
                return true;
            }

        }

        private string GenerateSecureCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);

            var code = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000;
            return code.ToString("D6");
        }

        private string GetMaskedEmail(string email)
        {
            if(string.IsNullOrEmpty(email))
                return "***";

            var parts=email.Split('@');
            if (parts.Length != 2)
                return "***";

            var localPart = parts[0];
            var domainPart = parts[1];

            if (localPart.Length <= 2)
                return $"**@{domainPart}";
            
            return $"{localPart[0]}***{localPart[^1]}@{domainPart}";
        }

        private async Task SendEmailAsync(string email, string code, string? firstName)
        {
            var subject = "Your Two-Factor Authentication Code";
            var body = $"Hello {firstName},\n\nYour two-factor authentication code is: " +
                $"{code}\n\nThis code will expire in 10 minutes.";

            await _emailService.SendEmailAsync(email, subject, body);
        }

        private string GetCacheKey(string userId)
        {
            return $"2fa:email:{userId}"; 
        }

        private string GenerateCode()
        {
            var random= new Random(); //не безопасно (поменять)
            return random.Next(100000, 999999).ToString();
        }

        private class TwoFactorCodeCache
        {
            public string Code { get; set; }
            public string UserId { get; set; }
            public int AttemptsRemaining { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}
