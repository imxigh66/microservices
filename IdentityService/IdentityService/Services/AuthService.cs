using IdentityService.DTO;
using IdentityService.Events;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityService.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly IConfiguration _configuration;
        private readonly ITwoFactorService _twoFactorService;
        private readonly IMemoryCache _cache;


        public AuthService(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            IEventPublisher eventPublisher,
            IConfiguration configuration,
            ITwoFactorService twoFactorService,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _eventPublisher = eventPublisher;
            _configuration = configuration;
            _twoFactorService = twoFactorService;
            _cache = cache;
        }
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user=await _userManager.FindByEmailAsync(request.Email);
            if(user==null || !user.IsActive)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Неверный email или пароль"
                };
            }

            var result= await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Неверный email или пароль"
                };
            }
            var twoFaEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

            if (twoFaEnabled)
            {
                // Определяем метод 2FA (можно хранить в профиле пользователя)
                var type = user.PreferredTwoFactorMethod ?? AuthenticationType.EmailCode;

                // Инициируем 2FA
                var twoFactorResult = await _twoFactorService.InitialTwoFactorAsync(user, type);

                if (!twoFactorResult.Success)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = twoFactorResult.Message
                    };
                }

              

                return new AuthResponse
                {
                    Success = false, // Еще не полностью аутентифицирован
                    RequiresTwoFactor = true,
                    TwoFactorToken = twoFactorResult.Token,
                    Type = type,
                    MaskedDestination = twoFactorResult.MaskedDestination,
                    Message = $"Код подтверждения отправлен на {twoFactorResult.MaskedDestination}"
                };
            }

            // Если 2FA не включена - продолжаем как раньше
            return await CompleteLoginAsync(user, request);
        }

        private async Task<AuthResponse> CompleteLoginAsync(ApplicationUser user, LoginRequest request)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            try
            {
                var userLoggedInEvent = new UserLoggedInEvent
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    LoggedInAt = user.LastLoginAt.Value
                    //IpAddress = request?.IpAddress ?? "unknown"
                };

                var topic = _configuration["Kafka:Topics:UserLoggedIn"];
                await _eventPublisher.PublishAsync(topic!, userLoggedInEvent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка публикации в Kafka: {ex.Message}");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles);

            return new AuthResponse
            {
                Success = true,
                Message = "Успешный вход",
                AccessToken = accessToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };
        }

        //private void SaveTwoFactorToken(string token, string userId)
        //{
        //    _cache.Set($"2fa:token:{token}", userId, TimeSpan.FromMinutes(10));
        //}


        public async Task<AuthResponse> LoginAfterTwoFactorAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Пользователь не найден"
                };
            }

            return await CompleteLoginAsync(user, new LoginRequest());
        }




        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser!=null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Пользователь с таким email уже существует"
                };
            }

            var user= new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PreferredTwoFactorMethod = AuthenticationType.EmailCode
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if(!result.Succeeded)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Ошибка при создании пользователя: " + string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            await _userManager.AddToRoleAsync(user, AppRoles.User);
            await _userManager.SetTwoFactorEnabledAsync(user, true);

            var roles = await _userManager.GetRolesAsync(user);

            try
            {
                var userRegisteredEvent = new UserRegisteredEvent
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    RegisteredAt = DateTime.UtcNow,
                    Roles = roles.ToList()
                };

                var topic = _configuration["Kafka:Topics:UserRegistered"];
                await _eventPublisher.PublishAsync(topic!, userRegisteredEvent);
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Ошибка публикации в Kafka: {ex.Message}");
            }

            return new AuthResponse
            {
                Success = true,
                Message = "Регистрация успешна"
            };
        }
    }
}
