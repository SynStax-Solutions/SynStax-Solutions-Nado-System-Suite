using FluentAssertions;
using Moq;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;
using Nado_Graphics_MS_Backend.Services;
using Xunit;

public class BillingStatusServiceTests
{
    private readonly Mock<IInvoiceRepository> _invoices = new();
    private readonly Mock<IQuoteRepository> _quotes = new();
    private BillingStatusService Sut => new(_invoices.Object, _quotes.Object);

    private static readonly DateTime Today = new(2026, 6, 15);
    //-------------------------------------------------------------------------------
    private void Candidates(params Invoice[] invoices) =>
        _invoices.Setup(r => r.GetOverdueCandidatesAsync(It.IsAny<DateTime>()))
                 .ReturnsAsync(invoices.ToList());
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Overdue_MarksInvoice_AndSavesOnce()
    {
        var invoice = new Invoice { InvoiceStatus = "Unpaid", DueDate = new DateTime(2026, 6, 1) };
        Candidates(invoice);

        var changed = await Sut.MarkOverdueInvoicesAsync(Today);

        changed.Should().Be(1);
        invoice.InvoiceStatus.Should().Be("Overdue");
        invoice.Notes.Should().Contain("01 Jun 2026");
        _invoices.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Overdue_KeepsThePreviousNote()
    {
        var invoice = new Invoice { InvoiceStatus = "Partially Paid", DueDate = new DateTime(2026, 6, 1), Notes = "Client will pay in two parts" };
        Candidates(invoice);

        await Sut.MarkOverdueInvoicesAsync(Today);

        invoice.Notes.Should().Contain("Client will pay in two parts");
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Overdue_NeverExceedsTheNotesColumnLimit()
    {
        var invoice = new Invoice { InvoiceStatus = "Unpaid", DueDate = new DateTime(2026, 6, 1), Notes = new string('x', 500) };
        Candidates(invoice);

        await Sut.MarkOverdueInvoicesAsync(Today);

        invoice.Notes!.Length.Should().BeLessOrEqualTo(500);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Overdue_NothingToDo_DoesNotSave()
    {
        Candidates();

        (await Sut.MarkOverdueInvoicesAsync(Today)).Should().Be(0);
        _invoices.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Overdue_SkipsInvoicesTheStateRefuses_AndStillProcessesTheRest()
    {
        var alreadyPaid = new Invoice { InvoiceStatus = "Paid", DueDate = new DateTime(2026, 6, 1) };
        var unknown = new Invoice { InvoiceStatus = "Banana", DueDate = new DateTime(2026, 6, 1) };
        var good = new Invoice { InvoiceStatus = "Unpaid", DueDate = new DateTime(2026, 6, 1) };
        Candidates(alreadyPaid, unknown, good);

        var changed = await Sut.MarkOverdueInvoicesAsync(Today);

        changed.Should().Be(1);
        alreadyPaid.InvoiceStatus.Should().Be("Paid");
        good.InvoiceStatus.Should().Be("Overdue");
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Overdue_AsksTheRepositoryForInvoicesDueBeforeToday()
    {
        Candidates();

        await Sut.MarkOverdueInvoicesAsync(new DateTime(2026, 6, 15, 14, 30, 0));

        _invoices.Verify(r => r.GetOverdueCandidatesAsync(new DateTime(2026, 6, 15)), Times.Once);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task ExpireQuotes_MarksQuoteExpired_AndKeepsPreviousNote()
    {
        var quote = new Quote { QuoteStatus = "Pending", ValidUntil = new DateTime(2026, 6, 1), Notes = "Rush job" };
        _quotes.Setup(r => r.GetExpiredPendingAsync(It.IsAny<DateTime>())).ReturnsAsync(new List<Quote> { quote });

        var changed = await Sut.ExpireOutdatedQuotesAsync(Today);

        changed.Should().Be(1);
        quote.QuoteStatus.Should().Be("Expired");
        quote.Notes.Should().Contain("Rush job").And.Contain("01 Jun 2026");
        _quotes.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task ExpireQuotes_SkipsQuotesTheStateRefuses()
    {
        var accepted = new Quote { QuoteStatus = "Accepted", ValidUntil = new DateTime(2026, 6, 1) };
        _quotes.Setup(r => r.GetExpiredPendingAsync(It.IsAny<DateTime>())).ReturnsAsync(new List<Quote> { accepted });

        (await Sut.ExpireOutdatedQuotesAsync(Today)).Should().Be(0);
        accepted.QuoteStatus.Should().Be("Accepted");
        _quotes.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
//--------------------------------------------------------------END OF FILE---------------------------------------------------------------