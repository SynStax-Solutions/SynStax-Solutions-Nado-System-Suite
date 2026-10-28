using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public interface IQuoteRepository : IRepository<Quote>
    {
        //-------------------------------------------------
        //All quotes with their items, newest first.
        Task<List<Quote>> GetAllWithItemsAsync();

        //-------------------------------------------------
        Task<Quote?> GetWithItemsAsync(int id);

        //-------------------------------------------------
        //Marks old line items for deletion (used when a quote's items are replaced).
        void RemoveItems(IEnumerable<QuoteItem> items);

        //-------------------------------------------------
        //Pending quotes whose ValidUntil date has passed.
        Task<List<Quote>> GetExpiredPendingAsync(DateTime validBefore);
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------