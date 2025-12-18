using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class AddToWishlistRequest
    {
        [Required]
        public int ProductId { get; set; } 
    }
}
