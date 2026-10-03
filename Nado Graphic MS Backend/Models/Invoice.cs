using Nado_Graphics_MS_Backend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nado_Graphics_MS_Backend.Models
{

    // EF Core entity mapped to dbo.Invoice. Implements IBillingDocument,
    // a shared interface with Quote for anything the two
    // documents have in common (reference number, status, total, etc.).
    public class Invoice: IBillingDocument
    {
        [Key]
        public int InvoiceID { get; set; }
        //-------------------------------------
        [Required]
        public int QuoteID { get; set; }
        //-------------------------------------
        [MaxLength(50)]
        [Required]
        public string ReferenceNumber { get; set; } = string.Empty;

        //-------------------------------------
        [Required]
        public int ClientID { get; set; }

        //-------------------------------------
        [Required]
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        //-------------------------------------
        [Required]
        public DateTime DueDate { get; set; }
        //-------------------------------------
        //Tracks the amount the client has to pay
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        //-------------------------------------
        //Tracks whether the client has paid yet, or cancelled their invoice
        [Required]
        [MaxLength(30)]
        public string InvoiceStatus { get; set; } = "Unpaid";

        //-------------------------------------
        //Tracks the amount the client has paid
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal AmountPaid { get; set; }
        //-------------------------------------
        [MaxLength(500)]
        public string? Notes { get; set; }
        //-------------------------------------
        //The products/services included on this invoice. This is required because an invoice can't have zero items.
        public List<InvoiceItem> InvoiceItems { get; set; } = new();
    }
}
//----------------------------------------------------END OF FILE--------------------------------------------------------