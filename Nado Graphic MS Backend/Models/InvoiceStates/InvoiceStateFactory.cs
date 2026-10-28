using Nado_Graphics_MS_Backend.Models.Interfaces;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Models.InvoiceStates
{
    // <summary>
    // Factory class to create instances of IInvoiceState based on the current state of an invoice.
    // </summary>
    public class InvoiceStateFactory
    {
        // <summary>
        // Returns an instance of IInvoiceState based on the current state of the provided invoice.
        // </summary>
        public static IInvoiceState GetState(Invoice invoice)
        {
            return invoice.InvoiceStatus switch
            {
                "Unpaid" => new UnpaidInvoiceState(),
                "Paid" => new PaidInvoiceState(),
                "Partially Paid" => new PartiallyPaidInvoiceState(),
                "Overdue" => new OverdueInvoiceState(invoice.Notes ?? string.Empty),
                "Cancelled" => new CancelledInvoiceState(invoice.Notes ?? string.Empty),
                "Extension Requested" => new ExtensionRequestedState(invoice.Notes ?? string.Empty),
                _ => throw new InvalidOperationException($"Unknown invoice status: {invoice.InvoiceStatus}")
            };
        }
    }
}
