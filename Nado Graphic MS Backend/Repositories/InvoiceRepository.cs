using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    //Data access for Invoice records. Inherits the generic CRUD methods from Repository<Invoice>
    //and adds invoice-specific queries, plus helpers for invoice items and payments
    public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
    {
        //The DbContext is injected by the DI container and passed to the base repository
        public InvoiceRepository(BMSDbContext context) : base(context) { }

        //---------------------------------------------------------------------------------------------------
        //Returns invoices that are past their due date and still open (unpaid, partially paid or
        //extension requested). Used by the billing check that marks invoices as overdue.
        public Task<List<Invoice>> GetOverdueCandidatesAsync(DateTime dueBefore)
        {
            //Status values are stored as strings in the database, so convert the enum values first
            var unpaid = States.InvoiceStatus.Unpaid.ToDbString();
            var partial = States.InvoiceStatus.PartiallyPaid.ToDbString();
            var extension = States.InvoiceStatus.ExtensionRequested.ToDbString();

            return _set
                .Where(i => i.DueDate < dueBefore
                         && (i.InvoiceStatus == unpaid || i.InvoiceStatus == partial || i.InvoiceStatus == extension))
                .ToListAsync();
        }

        //---------------------------------------------------------------------------------------------------
        //Returns all invoices with their line items loaded, newest invoice date first
        public Task<List<Invoice>> GetAllWithItemsAsync() =>
            _set.Include(i => i.InvoiceItems)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

        //---------------------------------------------------------------------------------------------------
        //Returns a single invoice by ID with its line items loaded, or null if it does not exist
        public Task<Invoice?> GetWithItemsAsync(int id) =>
            _set.Include(i => i.InvoiceItems).FirstOrDefaultAsync(i => i.InvoiceID == id);

        //---------------------------------------------------------------------------------------------------
        //Checks whether an invoice has already been created from the given quote
        public Task<bool> ExistsForQuoteAsync(int quoteId) =>
            _set.AnyAsync(i => i.QuoteID == quoteId);

        //---------------------------------------------------------------------------------------------------
        //Marks the given invoice items for deletion. The change is saved when SaveChanges is called
        public void RemoveItems(IEnumerable<InvoiceItem> items) =>
            _context.InvoiceItems.RemoveRange(items);

        //---------------------------------------------------------------------------------------------------
        //Adds a payment record to the context. The change is saved when SaveChanges is called
        public async Task AddPaymentAsync(Payment payment) =>
            await _context.Payments.AddAsync(payment);

        //---------------------------------------------------------------------------------------------------
        //Returns the invoices needed for a client statement: those belonging to the given clients,
        //excluding cancelled ones, and dated before the (exclusive) end date
        public Task<List<Invoice>> GetForStatementAsync(List<int> clientIds, string cancelledStatus, DateTime endExclusive) =>
        _set.Where(i => clientIds.Contains(i.ClientID)
                 && i.InvoiceStatus != cancelledStatus
                 && i.InvoiceDate < endExclusive)
        .ToListAsync();

        //---------------------------------------------------------------------------------------------------
        //Returns the payments made against the given invoices, dated before the (exclusive) end date.        
        public Task<List<Payment>> GetPaymentsForInvoicesAsync(List<int> invoiceIds, DateTime endExclusive) =>
            _context.Payments
                .Where(p => invoiceIds.Contains(p.InvoiceID) && p.PaymentDate < endExclusive)
                .ToListAsync();
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------