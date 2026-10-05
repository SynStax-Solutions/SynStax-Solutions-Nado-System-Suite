using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Frontend.Models
{
     //-----------------------------------------------------------------------------
    // Represents a Quote on the frontend/MVC side. Mirrors the shape of the
    // backend's Quote entity to bind form data and deserialize
    // API responses, but is kept separate from the backend model so the two
    // projects don't need a shared assembly reference.

    public class QuoteModel
    {
        //-----------------------------------------------------------------------------
        // Primary key. 0 when creating a new quote (not yet assigned by the DB);
        // populated when a quote is loaded back from the API for Edit/Details.
        public int QuoteID { get; set; }

        //-----------------------------------------------------------------------------
        // FK to the client this quote belongs to. Required because a quote
        // can't exist without an associated client.
        [Required(ErrorMessage = "Please select a client.")]
        public int ClientID { get; set; }

        //-----------------------------------------------------------------------------
        // Reference number shared across Quote/Invoice/Job
        // (e.g. "RIYA0001"). Defaulted to string.Empty rather than left null.
        public string? ReferenceNumber { get; set; } = string.Empty;

        //-----------------------------------------------------------------------------
        // When the quote was created. Set by the backend on creation,
        // the frontend doesn't need to populate this when submitting a new quote.
        public DateTime QuoteDate { get; set; }

        //-----------------------------------------------------------------------------
        // Optional expiry date the user picks on the Create/Edit form.
        // Nullable because a quote isn't required to have an expiry.
        public DateTime? ValidUntil { get; set; }

        //-----------------------------------------------------------------------------
        // Total cost of the quote. Always calculated server-side from the sum
        // of line items, is never trusted from what the frontend sends, so this
        // property exists to display the value once it comes back from
        // the API (e.g. on the Quotes index/details pages).
        public decimal TotalAmount { get; set; }

        //-----------------------------------------------------------------------------
        // Tracks quote lifecycle: Pending / Accepted / Rejected / Expired.
        // Defaults to "Pending" so a QuoteModel.
        public string QuoteStatus { get; set; } = "Pending";

        //-----------------------------------------------------------------------------
        // Optional text note attached to the quote.
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        //-----------------------------------------------------------------------------
        // The line items (products/services) on this quote. MinLength(1) gives client-side validation feedback
        //A quote must have at least one item - enforced again server-side.
        [MinLength(1, ErrorMessage = "Add at least one product to the quote.")]
        public List<QuoteItemModel> QuoteItems { get; set; } = new();

    }
}
//-------------------------------------------------------------END OF FILE-----------------------------------------------------------------------