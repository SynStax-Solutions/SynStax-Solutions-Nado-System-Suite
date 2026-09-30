using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nado_Graphics_MS_Backend.Models
{
    // EF Core entity mapped to dbo.QuoteItem.Represents a single product and
    // quantity line on a Quote.
    public class QuoteItem
    {
        //--------------------------------------------------------------------
        [Key]
        public int QuoteItemID { get; set; }

        //--------------------------------------------------------------------
        [Required]
        public int QuoteID { get; set; }

        //--------------------------------------------------------------------
        [Required]
        public int ProductID { get; set; }

        //--------------------------------------------------------------------
        [Required]
        public int Quantity { get; set; }

        //--------------------------------------------------------------------
        // the price used for this line item at the time the quote was created/edited.
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        //--------------------------------------------------------------------
        // Quantity * UnitPrice, calculated once server-side and stored so it
        // doesn't need to be recalculated every time the quote is displayed.
        [Column(TypeName = "decimal(10,2)")]
        public decimal LineTotal { get; set; }

        //Decided per line item by whoever builds the quote - whether this item's
        //listed price already includes VAT (used only to disclose the VAT portion,
        //never to change the line's price).
        public bool IsVatIncluded { get; set; }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------