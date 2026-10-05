using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Models.Interfaces;

namespace Nado_Graphics_MS_Backend.Models.QuoteStates
{
    // Represents a quote that has expired (its ValidUntil date passed, or it
    // was manually marked expired).
    public class ExpiredQuoteState: IQuoteState
    {
        // Stores the reason the quote was expired 
        public string Reason { get; }

        public ExpiredQuoteState(string reason) => Reason = reason;
        //----------------------------------------------------------------------------------
        public IQuoteState Accept(Quote quote)
        {
            throw new InvalidOperationException("Cannot accept an expired quote.");
        }
        //----------------------------------------------------------------------------------
        public IQuoteState Reject(Quote quote, string reason)
        {
            throw new InvalidOperationException("Cannot reject an expired quote.");
        }
        //----------------------------------------------------------------------------------
        public IQuoteState Expire(Quote quote, string reason)
        {
            throw new InvalidOperationException("Quote is already expired.");
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------