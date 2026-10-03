namespace Nado_Graphics_MS_Backend.Models.Interfaces
{
    public interface IInvoiceState
    {
        IInvoiceState Paid(Invoice invoice);

        IInvoiceState PartiallyPaid(Invoice invoice);
        IInvoiceState Overdue(Invoice invoice, string reason);
        IInvoiceState Cancelled(Invoice invoice, string reason);

        IInvoiceState ExtensionRequested(Invoice invoice, string reason);
    }
}
