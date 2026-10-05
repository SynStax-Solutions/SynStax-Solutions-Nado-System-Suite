using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Backend.Models
{
    public class CreateManualInvoiceRequest
    {
        [Required]
        public int ClientID { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required, MinLength(1)]
        public List<ManualInvoiceItemRequest> InvoiceItems { get; set; } = new();
    }

    public class ManualInvoiceItemRequest
    {
        [Required]
        public int ProductID { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        // Optional override; if null or <= 0, the product's current price is used.
        public decimal? UnitPrice { get; set; }
    }
}