using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.Models
{
	public class Payment
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string OrderId { get; set; } = string.Empty;

		[Required]
		public string UserId { get; set; } = string.Empty;

		[Column(TypeName = "decimal(18,2)")]
		public decimal Amount { get; set; }

		public PaymentStatus Status { get; set; }

		public string? StripeSessionId { get; set; }
		public string? StripePaymentIntentId { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? PaidAt { get; set; }
	}

}
