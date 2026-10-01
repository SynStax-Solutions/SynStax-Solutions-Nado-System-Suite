using Nado_Graphics_MS_Backend.Models.Interfaces;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Models.InvoiceStates
{
    // Represents an invoice that is overdue.
    public class OverdueInvoiceState: IInvoiceState
    {
        // Stores the reason the invoice is overdue
        public string Reason { get; }
        // Constructor to initialize the OverdueInvoiceState with a reason.
        public OverdueInvoiceState(string reason) => Reason = reason;
        // Being overdue does not prevent an invoice from being paid, so this method updates the invoice status to Paid and returns a new PaidInvoiceState.
        public IInvoiceState Paid(Invoice invoice)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.Paid.ToString();

            return new PaidInvoiceState();
        }
        // Being overdue does not prevent an invoice from being partially paid, so this method updates the invoice status to PartiallyPaid
        // and returns a new PartiallyPaidInvoiceState.
        public IInvoiceState PartiallyPaid(Invoice invoice)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.PartiallyPaid.ToString();

            return new PartiallyPaidInvoiceState();
        }
        // Throws an exception because an invoice that is already overdue cannot be marked as overdue again.
        public IInvoiceState Overdue(Invoice invoice, string reason)
        {
            throw new InvalidOperationException("Invoice is already overdued.");
        }
        // Being overdue does not prevent an invoice from being cancelled, so this method updates
        // the invoice status to Cancelled and returns a new CancelledInvoiceState.
        public IInvoiceState Cancelled(Invoice invoice, string reason)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.Cancelled.ToString();
            invoice.Notes = reason;

            return new CancelledInvoiceState(reason);
        }
        // Being overdue does not prevent an invoice from requesting an extension, so this method updates
        public IInvoiceState ExtensionRequested(Invoice invoice, string reason)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.ExtensionRequested.ToString();
            invoice.Notes = reason;

            return new ExtensionRequestedState(reason);
        }
    }
}
