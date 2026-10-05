using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Models.Interfaces;

namespace Nado_Graphics_MS_Backend.Models.QuoteStates
{
    // Represents a quote that has already been accepted.
    public class AcceptedQuoteState: IQuoteState
    {
        public IQuoteState Accept(Quote quote)
        {
            throw new InvalidOperationException("Quote is already accepted.");
        }
        //----------------------------------------------------------------------------------
        public IQuoteState Reject(Quote quote, string reason)
        {
            throw new InvalidOperationException("Cannot reject an accepted quote.");
        }
        //----------------------------------------------------------------------------------
        public IQuoteState Expire(Quote quote, string reason)
        {
            throw new InvalidOperationException("Cannot expire an accepted quote.");
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------