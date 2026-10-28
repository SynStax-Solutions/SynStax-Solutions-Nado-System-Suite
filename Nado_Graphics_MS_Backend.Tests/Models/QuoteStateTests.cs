using FluentAssertions;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Models.Interfaces;
using Nado_Graphics_MS_Backend.Models.QuoteStates;
using Xunit;

public class QuoteStateTests
{
    private static Quote QuoteWith(string status, string? notes = null) =>
        new() { QuoteStatus = status, Notes = notes };

    //-------------------------------------------------------------------------------
    private static IQuoteState Apply(IQuoteState state, string action, Quote quote) => action switch
    {
        "Accept" => state.Accept(quote),
        "Reject" => state.Reject(quote, "too expensive"),
        "Expire" => state.Expire(quote, "past valid date"),
        _ => throw new ArgumentException(action)
    };

    [Theory]
    //Pending: every transition is allowed
    [InlineData("Pending", "Accept", "Accepted")]
    [InlineData("Pending", "Reject", "Rejected")]
    [InlineData("Pending", "Expire", "Expired")]
    //Accepted, Rejected and Expired are terminal: nothing is allowed
    [InlineData("Accepted", "Accept", null)]
    [InlineData("Accepted", "Reject", null)]
    [InlineData("Accepted", "Expire", null)]
    [InlineData("Rejected", "Accept", null)]
    [InlineData("Rejected", "Reject", null)]
    [InlineData("Rejected", "Expire", null)]
    [InlineData("Expired", "Accept", null)]
    [InlineData("Expired", "Reject", null)]
    [InlineData("Expired", "Expire", null)]

    //-------------------------------------------------------------------------------
    public void Transition_FollowsTheRules(string from, string action, string? expectedStatus)
    {
        var quote = QuoteWith(from);
        var state = QuoteStateFactory.GetState(quote);

        if (expectedStatus == null)
        {
            var act = () => Apply(state, action, quote);
            act.Should().Throw<InvalidOperationException>();
            quote.QuoteStatus.Should().Be(from);
        }
        else
        {
            Apply(state, action, quote);
            quote.QuoteStatus.Should().Be(expectedStatus);
        }
    }

    //-------------------------------------------------------------------------------
    [Fact]
    public void Transition_ReturnsTheNewState()
    {
        QuoteStateFactory.GetState(QuoteWith("Pending")).Accept(QuoteWith("Pending"))
            .Should().BeOfType<AcceptedQuoteState>();
        QuoteStateFactory.GetState(QuoteWith("Pending")).Reject(QuoteWith("Pending"), "no")
            .Should().BeOfType<RejectedQuoteState>();
        QuoteStateFactory.GetState(QuoteWith("Pending")).Expire(QuoteWith("Pending"), "late")
            .Should().BeOfType<ExpiredQuoteState>();
    }

    //-------------------------------------------------------------------------------
    [Fact]
    public void Reject_StoresReasonInNotes()
    {
        var quote = QuoteWith("Pending");

        new PendingQuoteState().Reject(quote, "too expensive");

        quote.Notes.Should().Be("too expensive");
    }

    //-------------------------------------------------------------------------------
    [Fact]
    public void Factory_RestoresReasonFromNotes()
    {
        QuoteStateFactory.GetState(QuoteWith("Rejected", "too expensive"))
            .Should().BeOfType<RejectedQuoteState>()
            .Which.Reason.Should().Be("too expensive");
    }

    //-------------------------------------------------------------------------------
    [Fact]
    public void Factory_UnknownStatus_Throws()
    {
        var act = () => QuoteStateFactory.GetState(QuoteWith("Banana"));

        act.Should().Throw<InvalidOperationException>().WithMessage("*quote status*");
    }
}