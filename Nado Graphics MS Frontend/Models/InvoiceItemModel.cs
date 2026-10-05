using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Frontend.Models
{
    public class InvoiceItemModel
    {
        public int InvoiceItemID { get; set; }

        public int InvoiceID { get; set; }

        [Required(ErrorMessage = "Please select a product.")]
        public int ProductID { get; set; }

        public string? ProductName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative.")]
        public decimal UnitPrice { get; set; }

        public decimal LineTotal { get; set; }
    }
}
