using Nado_Graphics_MS_Backend.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Backend.Models.InvoiceStates
{
    public class ExtensionRequestedState: IInvoiceState
    {
        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public ExtensionRequestedState(string reason)
        {
            Reason = reason;
        }

        public IInvoiceState Paid(Invoice invoice)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.Paid.ToString();
            return new PaidInvoiceState();
        }

        public IInvoiceState PartiallyPaid(Invoice invoice)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.PartiallyPaid.ToString();
            return new PartiallyPaidInvoiceState();
        }

        public IInvoiceState Overdue(Invoice invoice, string reason)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.Overdue.ToString();
            invoice.Notes = reason;
            return new OverdueInvoiceState(reason);
        }

        public IInvoiceState Cancelled(Invoice invoice, string reason)
        {
            invoice.InvoiceStatus = States.InvoiceStatus.Cancelled.ToString();
            invoice.Notes = reason;
            return new CancelledInvoiceState(reason);
        }

        public IInvoiceState ExtensionRequested(Invoice invoice, string reason)
        {
            throw new InvalidOperationException("Invoice is already in extension requested state.");
        }
    }
}
