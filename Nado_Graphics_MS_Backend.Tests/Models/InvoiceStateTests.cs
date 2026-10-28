using FluentAssertions;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Models.Interfaces;
using Nado_Graphics_MS_Backend.Models.InvoiceStates;
using Xunit;

public class InvoiceStateTests
{
    private static Invoice InvoiceWith(string status, string? notes = null) =>
        new() { InvoiceStatus = status, Notes = notes };

    //-------------------------------------------------------------------------------
    private static IInvoiceState Apply(IInvoiceState state, string action, Invoice invoice) => action switch
    {
        "Paid" => state.Paid(invoice),
        "PartiallyPaid" => state.PartiallyPaid(invoice),
        "Overdue" => state.Overdue(invoice, "late"),
        "Cancelled" => state.Cancelled(invoice, "voided"),
        "ExtensionRequested" => state.ExtensionRequested(invoice, "needs time"),
        _ => throw new ArgumentException(action)
    };

    //expectedStatus = null means the transition must be refused.
    [Theory]
    //Unpaid: everything is allowed
    [InlineData("Unpaid", "Paid", "Paid")]
    [InlineData("Unpaid", "PartiallyPaid", "Partially Paid")]
    [InlineData("Unpaid", "Overdue", "Overdue")]
    [InlineData("Unpaid", "Cancelled", "Cancelled")]
    [InlineData("Unpaid", "ExtensionRequested", "Extension Requested")]
    //Partially Paid: everything is allowed
    [InlineData("Partially Paid", "Paid", "Paid")]
    [InlineData("Partially Paid", "PartiallyPaid", "Partially Paid")]
    [InlineData("Partially Paid", "Overdue", "Overdue")]
    [InlineData("Partially Paid", "Cancelled", "Cancelled")]
    [InlineData("Partially Paid", "ExtensionRequested", "Extension Requested")]
    //Paid: terminal, nothing is allowed
    [InlineData("Paid", "Paid", null)]
    [InlineData("Paid", "PartiallyPaid", null)]
    [InlineData("Paid", "Overdue", null)]
    [InlineData("Paid", "Cancelled", null)]
    [InlineData("Paid", "ExtensionRequested", null)]
    //Cancelled: terminal, nothing is allowed
    [InlineData("Cancelled", "Paid", null)]
    [InlineData("Cancelled", "PartiallyPaid", null)]
    [InlineData("Cancelled", "Overdue", null)]
    [InlineData("Cancelled", "Cancelled", null)]
    [InlineData("Cancelled", "ExtensionRequested", null)]
    //Overdue: can be paid, cancelled or extended, but not made overdue again
    [InlineData("Overdue", "Paid", "Paid")]
    [InlineData("Overdue", "PartiallyPaid", "Partially Paid")]
    [InlineData("Overdue", "Overdue", null)]
    [InlineData("Overdue", "Cancelled", "Cancelled")]
    [InlineData("Overdue", "ExtensionRequested", "Extension Requested")]
    //Extension Requested: can be paid, overdue or cancelled, but not extended twice
    [InlineData("Extension Requested", "Paid", "Paid")]
    [InlineData("Extension Requested", "PartiallyPaid", "Partially Paid")]
    [InlineData("Extension Requested", "Overdue", "Overdue")]
    [InlineData("Extension Requested", "Cancelled", "Cancelled")]
    [InlineData("Extension Requested", "ExtensionRequested", null)]


    //-------------------------------------------------------------------------------
    public void Transition_FollowsTheRules(string from, string action, string? expectedStatus)
    {
        var invoice = InvoiceWith(from);
        var state = InvoiceStateFactory.GetState(invoice);

        if (expectedStatus == null)
        {
            var act = () => Apply(state, action, invoice);
            act.Should().Throw<InvalidOperationException>();
            invoice.InvoiceStatus.Should().Be(from);   //a refused transition changes nothing
        }
        else
        {
            Apply(state, action, invoice);
            invoice.InvoiceStatus.Should().Be(expectedStatus);
        }
    }

    //-------------------------------------------------------------------------------
    [Fact]
    public void Transition_ReturnsTheNewState()
    {
        var invoice = InvoiceWith("Unpaid");
        var state = InvoiceStateFactory.GetState(invoice);

        state.PartiallyPaid(invoice).Should().BeOfType<PartiallyPaidInvoiceState>();
        state.Paid(invoice).Should().BeOfType<PaidInvoiceState>();
        state.Overdue(invoice, "late").Should().BeOfType<OverdueInvoiceState>();
        state.Cancelled(invoice, "void").Should().BeOfType<CancelledInvoiceState>();
        state.ExtensionRequested(invoice, "time").Should().BeOfType<ExtensionRequestedState>();
    }


    //-------------------------------------------------------------------------------
    [Fact]
    public void Overdue_StoresReason_InNotesAndInTheNewState()
    {
        var invoice = InvoiceWith("Unpaid");

        var next = (OverdueInvoiceState)InvoiceStateFactory.GetState(invoice).Overdue(invoice, "30 days late");

        invoice.Notes.Should().Be("30 days late");
        next.Reason.Should().Be("30 days late");
    }

    //-------------------------------------------------------------------------------
    [Fact]
    public void Factory_RestoresReasonFromNotes()
    {
        var state = InvoiceStateFactory.GetState(InvoiceWith("Cancelled", "client withdrew"));

        state.Should().BeOfType<CancelledInvoiceState>()
             .Which.Reason.Should().Be("client withdrew");
    }

    //-------------------------------------------------------------------------------
    [Fact]
    public void Factory_UnknownStatus_ThrowsWithInvoiceInTheMessage()
    {
        var act = () => InvoiceStateFactory.GetState(InvoiceWith("Banana"));

        act.Should().Throw<InvalidOperationException>().WithMessage("*invoice status*");
    }

    [Theory]
    [InlineData(States.InvoiceStatus.Unpaid, "Unpaid")]
    [InlineData(States.InvoiceStatus.PartiallyPaid, "Partially Paid")]
    [InlineData(States.InvoiceStatus.Paid, "Paid")]
    [InlineData(States.InvoiceStatus.Overdue, "Overdue")]
    [InlineData(States.InvoiceStatus.Cancelled, "Cancelled")]
    [InlineData(States.InvoiceStatus.ExtensionRequested, "Extension Requested")]

    //-------------------------------------------------------------------------------
    public void ToDbString_MatchesTheDatabaseCheckConstraint(States.InvoiceStatus status, string expected)
    {
        status.ToDbString().Should().Be(expected);
    }
}