using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Frontend.Models
{
    // Represents a single line item (one product/service and quantity) within a QuoteModel.
    //A quote can have many line items.
    public class QuoteItemModel
    {
        //-----------------------------------------------------------------------------
        // Primary key of the line item row.
        // 0 for a brand-new row that hasn't been saved yet.
        public int QuoteItemID { get; set; }

        //-----------------------------------------------------------------------------
        // FK back to the parent Quote. Set by the backend when items are
        // persisted; not meaningful on a new/unsaved item from the frontend.
        public int QuoteID { get; set; }


        //-----------------------------------------------------------------------------
        // Which product this line item refers to.
        // a blank value means the row is effectively empty and gets filtered out by
        // the controller before submission.
        [Required(ErrorMessage = "Please select a product.")]
        public int ProductID { get; set; }

        //-----------------------------------------------------------------------------
        //Populated when returned from the API so the view can display a name
        //without a separate lookup
        //not sent when creating/editing.
        public string? ProductName { get; set; }

        //-----------------------------------------------------------------------------
        // How many units of the product are on this line. Defaults to 1 since
        // that's the normal case when a user adds a new row.
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } = 1;

        //-----------------------------------------------------------------------------
        //Optional, if left as 0, the server uses the product's current listed price.
        [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative.")]
        public decimal? UnitPrice { get; set; }

        //-----------------------------------------------------------------------------
        public bool IsVatIncluded { get; set; }

        //-----------------------------------------------------------------------------
        // Quantity * UnitPrice for this line. Calculated and returned by the backend
        public decimal LineTotal { get; set; }
    }
}
//-------------------------------------------------------------END OF FILE-----------------------------------------------------------------------