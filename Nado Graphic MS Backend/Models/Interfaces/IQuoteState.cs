using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Models.Interfaces
{
    // Defines the State design pattern's contract for a quote's lifecycle.
    // Each state (Pending/Accepted/Rejected/Expired) implements this interface
    // and decides for itself whether a given transition is
    // legal from that state - e.g. PendingQuoteState.Accept() succeeds, but
    // AcceptedQuoteState.Accept() throws, because you can't accept something
    // that's already accepted.

    // Each method returns IQuoteState (the new state after a successful
    // transition) rather than void
    public interface IQuoteState
    {
        IQuoteState Accept(Quote quote);

        IQuoteState Reject(Quote quote, string reason);

        IQuoteState Expire(Quote quote, string reason);
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------