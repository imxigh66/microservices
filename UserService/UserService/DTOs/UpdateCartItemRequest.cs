using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class UpdateCartItemRequest
    {
        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }

        public string? Size { get; set; }
        public string? Color { get; set; }
    }
}
