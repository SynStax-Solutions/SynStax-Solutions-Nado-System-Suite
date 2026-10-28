using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        //-----------------------------------------------------------
        //All invoices with their items, newest first.
        Task<List<Invoice>> GetAllWithItemsAsync();

        //-----------------------------------------------------------
        Task<Invoice?> GetWithItemsAsync(int id);

        //-----------------------------------------------------------
        //True if an invoice was already created from this quote.
        Task<bool> ExistsForQuoteAsync(int quoteId);

        //-----------------------------------------------------------
        void RemoveItems(IEnumerable<InvoiceItem> items);

        //-----------------------------------------------------------
        //Payments belong to an invoice, so they are stored through the repository.
        Task AddPaymentAsync(Payment payment);

        //-----------------------------------------------------------
        //Non-cancelled invoices for these clients dated before endExclusive
        Task<List<Invoice>> GetForStatementAsync(List<int> clientIds, string cancelledStatus, DateTime endExclusive);

        //-----------------------------------------------------------
        //Payments on these invoices made before endExclusive.
        Task<List<Payment>> GetPaymentsForInvoicesAsync(List<int> invoiceIds, DateTime endExclusive);

        //-----------------------------------------------------------
        //Invoices that are past their due date but not yet marked Overdue
        //(Unpaid, Partially Paid or Extension Requested).
        Task<List<Invoice>> GetOverdueCandidatesAsync(DateTime dueBefore);
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------