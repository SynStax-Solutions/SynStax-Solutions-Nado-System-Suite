using Nado_Graphics_MS_Backend.Models.Interfaces;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Models.InvoiceStates
{
    // Represents the state of an invoice that has not been paid yet
    public class UnpaidInvoiceState: IInvoiceState
    {
        // Transitions the invoice to the Paid state, updates the invoice status, and returns a new PaidInvoiceState.
        public IInvoiceState Paid(Invoice invoice)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.Paid.ToDbString();

            return new PaidInvoiceState();
        }
        // Transitions the invoice to the Partially Paid state, updates the invoice status, and returns a new PartiallyPaidInvoiceState.
        public IInvoiceState PartiallyPaid(Invoice invoice)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.PartiallyPaid.ToDbString();

            return new PartiallyPaidInvoiceState();
        }

        // Transitions the invoice to the Overdue state, updates the invoice status and notes, and returns a new OverdueInvoiceState.
        public IInvoiceState Overdue(Invoice invoice, string reason)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.Overdue.ToDbString();
            invoice.Notes = reason;

            return new OverdueInvoiceState(reason);
        }

        // Transitions the invoice to the Cancelled state, updates the invoice status and notes, and returns a new CancelledInvoiceState.
        public IInvoiceState Cancelled(Invoice invoice, string reason)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.Cancelled.ToDbString();
            invoice.Notes = reason;

            return new CancelledInvoiceState(reason);
        }

        // Transitions the invoice to the Extension Requested state, updates the invoice status and notes, and returns a new ExtensionRequestedState.
        public IInvoiceState ExtensionRequested(Invoice invoice, string reason)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.ExtensionRequested.ToDbString();
            invoice.Notes = reason;
            return new ExtensionRequestedState(reason);
        }
    }
}
