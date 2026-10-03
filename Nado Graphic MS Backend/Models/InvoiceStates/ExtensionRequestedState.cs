using Nado_Graphics_MS_Backend.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Backend.Models.InvoiceStates
{
    public class ExtensionRequestedState: IInvoiceState
    {
        
        public string Reason { get; }

        public ExtensionRequestedState(string reason)
        {
            Reason = reason;
        }

        public IInvoiceState Paid(Invoice invoice)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.Paid.ToDbString();
            return new PaidInvoiceState();
        }

        public IInvoiceState PartiallyPaid(Invoice invoice)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.PartiallyPaid.ToDbString();
            return new PartiallyPaidInvoiceState();
        }

        public IInvoiceState Overdue(Invoice invoice, string reason)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.Overdue.ToDbString();
            invoice.Notes = reason;
            return new OverdueInvoiceState(reason);
        }

        public IInvoiceState Cancelled(Invoice invoice, string reason)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.Cancelled.ToDbString();
            invoice.Notes = reason;
            return new CancelledInvoiceState(reason);
        }

        public IInvoiceState ExtensionRequested(Invoice invoice, string reason)
        {
            throw new InvalidOperationException("Invoice is already in extension requested state.");
        }
    }
}
