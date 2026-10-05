using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Frontend.Models
{
    // What the Record Payment form binds to. The Backend sets the payment date itself.
    public class RecordPaymentModel
    {
        [Required]
        public int InvoiceID { get; set; }

        [Required(ErrorMessage = "Enter the amount paid")]
        [Range(0.01, 100000000, ErrorMessage = "Enter an amount greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Choose a payment method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the transaction reference")]
        [MaxLength(100)]
        public string TransactionReference { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}