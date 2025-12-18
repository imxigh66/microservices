using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class UpdateProfileRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }


        [Phone]
        public string? PhoneNumber { get; set; }
    }
}
