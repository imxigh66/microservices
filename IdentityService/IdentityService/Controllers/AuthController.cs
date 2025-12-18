using IdentityService.DTO;
using IdentityService.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
       private readonly IAuthService _authService;
        private readonly ITwoFactorService _twoFactorService;
        public AuthController(IAuthService authService,ITwoFactorService twoFactorService)
        {
            _authService = authService;
            _twoFactorService = twoFactorService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _authService.RegisterAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            //request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.LoginAsync(request);

            if (result.RequiresTwoFactor)
            {
                
                return Ok(new
                {
                    requiresTwoFactor = true,
                    twoFactorToken = result.TwoFactorToken,
                    method = result.Type.ToString(),
                    destination = result.MaskedDestination,
                    message = result.Message
                });
            }

            if (!result.Success)
            {
                return Unauthorized(new { message = result.Message });
            }

            return Ok(result);
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] TwoFactorVerifyRequest request)
        {
            var context = _twoFactorService.GetContext(request.Token);
            if (context == null)
            {
                return Unauthorized(new { message = "Сессия двухфакторной авторизации истекла или не найдена." });
            }
            // Проверяем код
            var isValid = await _twoFactorService.VerifyTwoFactorAsync(request.Token, request.Code);

            if (!isValid)
            {
                return Unauthorized(new { message = "Неверный или истекший код подтверждения." });
            }

           
            // Завершаем логин (создаем токен и возвращаем пользователю)
            var result = await _authService.LoginAfterTwoFactorAsync(context.UserId);

            return Ok(result);
        }

    }
}
