using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nado_Graphics_MS_Backend.Models
{
    // Only temporary class to store payment information, due to the fact that payments are not stored in the database yet
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }
        [Required]
        public int InvoiceID { get; set; }
        public Invoice Invoice { get; set; } = null!;
        [Required]
        public int ClientID { get; set; }
        [Required]
        public string ReferenceNumber { get; set; } = string.Empty;
        [Required]
        public DateTime PaymentDate { get; set; }
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }
        [Required]
        public PaymentMethodOptions.PaymentMethod PaymentMethod { get; set; }
        [Required]
        public string TransactionReference { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Notes;
    }
}
