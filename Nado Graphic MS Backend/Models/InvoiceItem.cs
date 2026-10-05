using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nado_Graphics_MS_Backend.Models
{
    public class InvoiceItem
    {
        [Key]
        public int InvoiceItemID { get; set; }
        //--------------------------------------------------------------------
        [Required]
        public int InvoiceID { get; set; }
        //--------------------------------------------------------------------
        [Required]
        public int ProductID { get; set; }

        //--------------------------------------------------------------------
        [Required]
        public int Quantity { get; set; }

        //--------------------------------------------------------------------
        // the price used for this line item at the time the invoice was created/edited.
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        //--------------------------------------------------------------------
        // Quantity * UnitPrice, calculated once server-side and stored so it
        // doesn't need to be recalculated every time the invoice is displayed.
        [Column(TypeName = "decimal(10,2)")]
        public decimal LineTotal
        {
            get; set;
        }
    }
}
