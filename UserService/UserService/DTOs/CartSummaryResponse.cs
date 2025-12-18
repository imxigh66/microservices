namespace UserService.DTOs
{
    public class CartSummaryResponse
    {
        public int TotalItems { get; set; }
        public decimal TotalPrice { get; set; }
        public List<CartItemResponse> Items { get; set; } = new();
    }
}
