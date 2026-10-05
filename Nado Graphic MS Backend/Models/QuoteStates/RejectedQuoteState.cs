using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Models.Interfaces;

namespace Nado_Graphics_MS_Backend.Models.QuoteStates
{
    // Represents a quote the client has rejected.
    // nothing about the quote can change via these transitions (a new quote would need to be created instead).
    public class RejectedQuoteState : IQuoteState
    {
        // The reason the client gave for rejecting
        public string Reason { get; }

        public RejectedQuoteState(string reason) => Reason = reason;
        //----------------------------------------------------------------------------------
        public IQuoteState Accept(Quote quote)
        {
            throw new InvalidOperationException("Cannot accept a rejected quote.");
        }
        //----------------------------------------------------------------------------------
        public IQuoteState Reject(Quote quote, string reason)
        {
            throw new InvalidOperationException("Quote is already rejected.");
        }
        //----------------------------------------------------------------------------------
        public IQuoteState Expire(Quote quote, string reason)
        {
            throw new InvalidOperationException("Cannot expire a rejected quote.");
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------