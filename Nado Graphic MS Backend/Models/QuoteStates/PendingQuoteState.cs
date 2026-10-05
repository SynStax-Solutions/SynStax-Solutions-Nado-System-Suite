using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Models.Interfaces;

namespace Nado_Graphics_MS_Backend.Models.QuoteStates
{
        //The starting state for every new quote. This is the only state where
        //a transition actually succeeds - every other state is terminal.
        public class PendingQuoteState : IQuoteState
        {
            //Moves the quote to Accepted and returns the new state.
            public IQuoteState Accept(Quote quote)
            {
                quote.QuoteStatus = States.QuoteStatus.Accepted.ToString();

                return new AcceptedQuoteState();
            }
            //----------------------------------------------------------------------------------
            //Moves the quote to Rejected, records the reason, and returns the new state.
            public IQuoteState Reject(Quote quote, string reason)
            {
                quote.QuoteStatus = States.QuoteStatus.Rejected.ToString();
                quote.Notes = reason;

                return new RejectedQuoteState(reason);
            }
            //----------------------------------------------------------------------------------
            //Moves the quote to Expired, records the reason, and returns the new state.
            public IQuoteState Expire(Quote quote, string reason)
            {
                quote.QuoteStatus = States.QuoteStatus.Expired.ToString();
                quote.Notes = reason;

                return new ExpiredQuoteState(reason);
            }
        }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------