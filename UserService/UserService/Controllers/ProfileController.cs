using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController:ControllerBase
    {
        private readonly IProfileService _profileService;
        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("uid");

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "UserId не найден в токене" });

            var profile = await _profileService.GetProfileAsync(userId);
            if (profile == null)
                return NotFound(new { message = "Профиль не найден" });
            return Ok(profile);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProfileById(string userId)
        {
            var profile = await _profileService.GetProfileAsync(userId);

            if (profile == null)
                return NotFound(new { message = "Профиль не найден" });

            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("uid");

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "UserId не найден в токене" });

            var profile = await _profileService.UpdateProfileAsync(userId, request);

            if (profile == null)
                return NotFound(new { message = "Профиль не найден" });

            return Ok(profile);
        }

        [HttpGet("/api/health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", service = "UserService", timestamp = DateTime.UtcNow });
        }
    }
}
