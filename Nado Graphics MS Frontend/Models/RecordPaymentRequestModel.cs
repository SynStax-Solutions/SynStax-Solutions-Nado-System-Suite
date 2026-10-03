using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Frontend.Models
{
    public class RecordPaymentRequestModel
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "Cash";

        [MaxLength(100)]
        public string? TransactionReference { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
//--------------------------------------------------END OF FILE-----------------------------------------------------------