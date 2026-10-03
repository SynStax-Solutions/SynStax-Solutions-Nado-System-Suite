using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Nado_Graphics_MS_Backend.Models
{

    // Groups the two status enums used across the billing-document state
    // machines. These exist mainly so QuoteStates classes (PendingQuoteState,
    // etc.) can reference States.QuoteStatus.Accepted.ToString() instead of
    // hardcoding the raw string "Accepted" everywhere
    //
    // if a status name ever needs to change, there's one place to update it 
    // (though the DB CHECK constraints and any hardcoded strings elsewhere would also need
    // updating).
    public class States
    {
        //-------------------------------------------------------------------------------------
        //Matches CONSTRAINT Check_Quote_Status: 'Pending', 'Accepted', 'Rejected', 'Expired'.
        public enum QuoteStatus
        {
            Pending,
            Accepted,
            Rejected,
            Expired
        }

        //-------------------------------------------------------------------------------------
        //Matches CONSTRAINT CK_Invoice_Status: 'Unpaid', 'Partially Paid', 'Paid', 'Overdue', 'Cancelled'
        //But also includes ExtensionRequested, which is not a valid status for the invoice itself, but is used to track when an extension request has been made.
        public enum InvoiceStatus
        {
            Unpaid,

            PartiallyPaid,
            Paid,
            Overdue,
            Cancelled,
            ExtensionRequested
        }
    }

    //InvoiceStatus.ToString() doesn't match the CHECK constraint for "Partially Paid"
    //and "Extension Requested" (no space vs a space) - this is the single source of
    //truth for converting a status enum to the exact string the database expects.
    //Use this everywhere instead of calling .ToString() directly on InvoiceStatus.
    public static class InvoiceStatusExtensions
    {
        public static string ToDbString(this States.InvoiceStatus status) => status switch
        {
            States.InvoiceStatus.PartiallyPaid => "Partially Paid",
            States.InvoiceStatus.ExtensionRequested => "Extension Requested",
            _ => status.ToString()
        };
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------