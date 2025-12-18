using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTO
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
