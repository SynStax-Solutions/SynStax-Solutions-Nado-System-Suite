using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Backend.Models
{
    // Represents a request to record a payment for an invoice. This is temporary class to store payment information,
    // due to the fact that payments are not stored in the database yet.
    public class RecordPaymentRequest
    {
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive value.")]
        public decimal Amount { get; set; }

        public PaymentMethodOptions.PaymentMethod PaymentMethod { get; set; }

        public string TransactionReference { get; set; } = string.Empty;
        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string? Notes { get; set; }
    }
}
