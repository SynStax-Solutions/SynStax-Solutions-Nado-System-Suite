using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public class QuoteRepository : Repository<Quote>, IQuoteRepository
    {
        public QuoteRepository(BMSDbContext context) : base(context) { }

        //-------------------------------------------------------------------------------
        public Task<List<Quote>> GetAllWithItemsAsync() =>
            _set.Include(q => q.QuoteItems)
                .OrderByDescending(q => q.QuoteDate)
                .ToListAsync();
        //-------------------------------------------------------------------------------
        public Task<Quote?> GetWithItemsAsync(int id) =>
            _set.Include(q => q.QuoteItems).FirstOrDefaultAsync(q => q.QuoteID == id);
        //-------------------------------------------------------------------------------
        public void RemoveItems(IEnumerable<QuoteItem> items) =>
            _context.QuoteItems.RemoveRange(items);
        //-------------------------------------------------------------------------------
        public Task<List<Quote>> GetExpiredPendingAsync(DateTime validBefore)
        {
            var pending = States.QuoteStatus.Pending.ToString();

            return _set
                .Where(q => q.QuoteStatus == pending && q.ValidUntil != null && q.ValidUntil < validBefore)
                .ToListAsync();
        }
        //-------------------------------------------------------------------------------
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------