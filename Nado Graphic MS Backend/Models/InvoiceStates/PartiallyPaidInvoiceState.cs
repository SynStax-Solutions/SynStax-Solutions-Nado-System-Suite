using Nado_Graphics_MS_Backend.Models.Interfaces;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Models.InvoiceStates
{
    // Represents an invoice that has been partially paid.
    public class PartiallyPaidInvoiceState: IInvoiceState
    {
        // The Paid method transitions the invoice to a fully paid state, updating the invoice status and returning a new PaidInvoiceState.
        // This happens when the remaining balance of the invoice is paid in full.
        // This is represented when AmountPaid is equal to the TotalAmount of the invoice.
        public IInvoiceState Paid(Invoice invoice)
        {
            
            invoice.InvoiceStatus = States.InvoiceStatus.Paid.ToDbString();

            return new PaidInvoiceState();
        }
        // The PartiallyPaid method transitions the invoice to a partially paid state, updating the invoice status and returning a new PartiallyPaidInvoiceState
        // This is represented when AmountPaid is less than the TotalAmount of the invoice.
        // The reason why partially paid invoices can be marked as partially paid is to allow for flexibility in payment arrangements,
        // such as installment plans or partial payments, which are common in business transactions.
        public IInvoiceState PartiallyPaid(Invoice invoice)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.PartiallyPaid.ToDbString();

            return new PartiallyPaidInvoiceState();
        }

        // The Overdue method transitions the invoice to an overdue state, updating the invoice status and notes with the provided reason,
        // and returning a new OverdueInvoiceState. This is used when the invoice has not been paid by its due date.
        // The reason why partially paid invoices can be marked as overdue is to reflect the reality that even if a partial payment has been made,
        // the invoice is still past its due date.
        public IInvoiceState Overdue(Invoice invoice, string reason)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.Overdue.ToDbString();
            invoice.Notes = reason;

            return new OverdueInvoiceState(reason);
        }

        // The Cancelled method transitions the invoice to a cancelled state, updating the invoice status and notes with the provided reason,
        // and returning a new CancelledInvoiceState. This is used when the invoice is no longer valid or has been voided.
        // The reason why partially paid invoices can be marked as cancelled is to reflect the reality that the invoice is no longer valid or has been voided.
        public IInvoiceState Cancelled(Invoice invoice, string reason)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.Cancelled.ToDbString();
            invoice.Notes = reason;

            return new CancelledInvoiceState(reason);
        }
        // The ExtensionRequested method transitions the invoice to an extension requested state, updating the invoice status and notes with the provided reason,
        // and returning a new ExtensionRequestedState. This is used when the invoice's due date has been extended.
        // The reason why partially paid invoices can request an extension is to reflect the reality that the invoice's due date has been extended.
        public IInvoiceState ExtensionRequested(Invoice invoice, string reason)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.ExtensionRequested.ToDbString();
            invoice.Notes = reason;
            return new ExtensionRequestedState(reason);
        }
    }
}
