namespace UserService.Models
{
    public enum OrderStatus
    {
        Pending = 0,       // Ожидает обработки
        Processing = 1,    // В обработке
        Shipped = 2,       // Отправлен
        Delivered = 3,     // Доставлен
        Cancelled = 4      // Отменен
    }
}
