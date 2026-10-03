using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Frontend.Models
{
    public class InvoiceModel
    {
        public int InvoiceID { get; set; }

        //Nullable - an invoice doesn't need a linked quote.
        public int? QuoteID { get; set; }

        public string? ReferenceNumber { get; set; }

        [Required(ErrorMessage = "Please select a client.")]
        public int ClientID { get; set; }

        public DateTime InvoiceDate { get; set; }

        public DateTime DueDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string InvoiceStatus { get; set; } = "Unpaid";

        public decimal AmountPaid { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MinLength(1, ErrorMessage = "Add at least one product to the invoice.")]
        public List<InvoiceItemModel> InvoiceItems { get; set; } = new();
    }
}
