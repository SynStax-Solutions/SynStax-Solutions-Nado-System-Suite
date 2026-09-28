using Nado_Graphics_MS_Backend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nado_Graphics_MS_Backend.Models
{
    // EF Core entity mapped to dbo.Quote. Implements IBillingDocument,
    // a shared interface with Invoice for anything the two
    // documents have in common (reference number, status, total, etc.).
    public class Quote: IBillingDocument
    {
        //---------------------------------------------------------------
        [Key]
        public int QuoteID { get; set; }

        //---------------------------------------------------------------
        //Foreign key to the client this quote is for.
        [Required]
        public int ClientID { get; set; }

        //---------------------------------------------------------------
        //Shared reference number between the quote, invoice and job.
        [MaxLength(50)]
        public string ReferenceNumber { get; set; } = string.Empty;

        //---------------------------------------------------------------
        public DateTime QuoteDate { get; set; } = DateTime.Now;

        //---------------------------------------------------------------
        //Until when the quote is valid before it expires.
        public DateTime? ValidUntil { get; set; }

        //----------------------------------------------------------------
        // Total cost of the quote, stored as a fixed decimal(10,2) to avoid
        // floating point rounding issues with money. 
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        //----------------------------------------------------------------
        //Tracks whether the client accepts or rejects the quote. Defaults to Pending.
        [Required]
        [MaxLength(30)]
        public string QuoteStatus { get; set; } = "Pending";

        //----------------------------------------------------------------
        [MaxLength(500)]
        public string? Notes { get; set; }

        //----------------------------------------------------------------
        //The products/services included on this quote. This is required because a quote can't have zero items.
        public List<QuoteItem> QuoteItems { get; set; } = new();

    }
}
//----------------------------------------------------END OF FILE--------------------------------------------------------