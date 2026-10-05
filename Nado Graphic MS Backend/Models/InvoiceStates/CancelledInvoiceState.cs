using Nado_Graphics_MS_Backend.Models.Interfaces;

namespace Nado_Graphics_MS_Backend.Models.InvoiceStates
{
    // Represents an invoice that has been cancelled.
    public class CancelledInvoiceState : IInvoiceState
    {
        // Stores the reason the invoice was cancelled
        public string Reason { get; }

        // Constructor to initialize the CancelledInvoiceState with a reason.
        public CancelledInvoiceState(string reason) => Reason = reason;
        // Throws an exception because a cancelled invoice cannot be marked as paid.
        public IInvoiceState Paid(Invoice invoice)
        {
            throw new InvalidOperationException("Invoice has been cancelled, so it can't be fully paid for.");
        }
        // Throws an exception because a cancelled invoice cannot be marked as partially paid.
        public IInvoiceState PartiallyPaid(Invoice invoice)
        {
            throw new InvalidOperationException("Invoice has been cancelled, thus can't be paid for now.");
        }
        // Throws an exception because a cancelled invoice cannot be marked as overdue.
        public IInvoiceState Overdue(Invoice invoice, string reason)
        {
            throw new InvalidOperationException("Invoice can't be overdue as it has been cancelled");
        }
        // Throws an exception because a cancelled invoice cannot be cancelled again.
        public IInvoiceState Cancelled(Invoice invoice, string reason)
        {
            throw new InvalidOperationException("Invoice has already been cancelled, so it can't be cancelled again.");
        }
        // Throws an exception because a cancelled invoice cannot request an extension.
        public IInvoiceState ExtensionRequested(Invoice invoice, string reason)
        {
            throw new InvalidOperationException("Invoice has been cancelled, so it can't request an extension.");
        }
    }
}
