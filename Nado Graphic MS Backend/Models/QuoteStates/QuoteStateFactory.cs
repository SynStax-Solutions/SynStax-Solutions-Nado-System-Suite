using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Models.Interfaces;

namespace Nado_Graphics_MS_Backend.Models.QuoteStates
{
    //Converts the QuoteStatus string stored in the database into the matching
    //state object, so business logic can ask "is this transition allowed?"
    public class QuoteStateFactory
    {
        public static IQuoteState GetState(Quote quote)
        {
            return quote.QuoteStatus switch
            {
                "Pending" => new PendingQuoteState(),
                "Accepted" => new AcceptedQuoteState(),
                "Rejected" => new RejectedQuoteState(quote.Notes ?? string.Empty),
                "Expired" => new ExpiredQuoteState(quote.Notes ?? string.Empty),
                _ => throw new InvalidOperationException($"Unknown quote status: {quote.QuoteStatus}")
            };
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------