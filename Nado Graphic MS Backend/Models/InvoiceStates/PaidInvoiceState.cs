using Nado_Graphics_MS_Backend.Models.Interfaces;

namespace Nado_Graphics_MS_Backend.Models.InvoiceStates
{
    // Represents the state of an invoice that has been fully paid
    public class PaidInvoiceState: IInvoiceState
    {
        // Throws an exception if an attempt is made to transition to the Paid state, as the invoice is already fully paid
        public IInvoiceState Paid(Invoice invoice)
        {
            throw new InvalidOperationException("Invoice has already been fully paid");
        }
        // Throws an exception if an attempt is made to transition to the Partially Paid state, as the invoice is already fully paid
        public IInvoiceState PartiallyPaid(Invoice invoice)
        {
            throw new InvalidOperationException("Invoice has already been fully paid, so there's nothing more to pay.");
        }
        // Throws an exception if an attempt is made to transition to the Overdue state, as the invoice is already fully paid
        public IInvoiceState Overdue(Invoice invoice, string reason)
        {
            throw new InvalidOperationException("Invoice has already been paid, so even if it was overdued, it's been paid for.");
        }
        // Throws an exception if an attempt is made to transition to the Cancelled state, as the invoice is already fully paid
        public IInvoiceState Cancelled(Invoice invoice, string reason)
        {
            throw new InvalidOperationException("Invoice is already paid for, so it can't be cancelled.");
        }
        // Throws an exception if an attempt is made to transition to the Extension Requested state, as the invoice is already fully paid
        public IInvoiceState ExtensionRequested(Invoice invoice, string reason)
        {
            throw new InvalidOperationException("Invoice has already been paid for, so it can't request an extension.");
        }
    }
}
