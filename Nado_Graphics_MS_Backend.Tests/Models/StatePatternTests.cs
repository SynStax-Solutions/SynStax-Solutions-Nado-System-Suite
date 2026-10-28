using FluentAssertions;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Models.QuoteStates;
using Xunit;

public class StatePatternTests
{
    private static Quote QuoteWith(string status) => new() { QuoteStatus = status };
    //-------------------------------------------------------------------------------
    [Fact]
    public void Factory_Pending_ReturnsPendingState()
        => QuoteStateFactory.GetState(QuoteWith("Pending")).Should().BeOfType<PendingQuoteState>();

    //-------------------------------------------------------------------------------
    [Fact]
    public void Factory_UnknownStatus_Throws()
    {
        var act = () => QuoteStateFactory.GetState(QuoteWith("Banana"));
        act.Should().Throw<InvalidOperationException>();
    }

    //-------------------------------------------------------------------------------
    [Fact]
    public void Pending_Accept_SetsAcceptedStatus()
    {
        var q = QuoteWith("Pending");
        var next = new PendingQuoteState().Accept(q);
        q.QuoteStatus.Should().Be("Accepted");
        next.Should().BeOfType<AcceptedQuoteState>();
    }

    //-------------------------------------------------------------------------------
    [Fact]
    public void Pending_Reject_RecordsReason()
    {
        var q = QuoteWith("Pending");
        new PendingQuoteState().Reject(q, "Too expensive");
        q.QuoteStatus.Should().Be("Rejected");
        q.Notes.Should().Be("Too expensive");
    }

    //-------------------------------------------------------------------------------
    [Fact]
    public void Pending_Expire_RecordsReason()
    {
        var q = QuoteWith("Pending");
        new PendingQuoteState().Expire(q, "Past valid-until date");
        q.QuoteStatus.Should().Be("Expired");
    }

    //-------------------------------------------------------------------------------
    [Fact]
    public void Accepted_CannotBeAcceptedAgain()
    {
        var q = QuoteWith("Accepted");
        var act = () => new AcceptedQuoteState().Accept(q);
        act.Should().Throw<Exception>();
    }
}