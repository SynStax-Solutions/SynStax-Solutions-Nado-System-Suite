using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;
using Nado_Graphics_MS_Backend.Services;
using Xunit;

public class BillingStatusIntegrationTests : IDisposable
{
    private readonly TestDb _db = new();
    public void Dispose() => _db.Dispose();

    private BillingStatusService Sut => new(new InvoiceRepository(_db.Ctx), new QuoteRepository(_db.Ctx));
    //-------------------------------------------------------------------------------
    private Invoice NewInvoice(string reference, string status, DateTime dueDate) => new()
    {
        ClientID = _db.ClientId,
        ReferenceNumber = reference,
        InvoiceStatus = status,
        InvoiceDate = DateTime.Today.AddDays(-40),
        DueDate = dueDate,
        TotalAmount = 100m,
        AmountPaid = 0
    };
    //-------------------------------------------------------------------------------
    private string StatusOf(string reference) =>
        _db.Ctx.Invoices.AsNoTracking().Single(i => i.ReferenceNumber == reference).InvoiceStatus;
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task MarkOverdue_ChangesOnlyEligibleInvoices()
    {
        var past = DateTime.Today.AddDays(-5);
        _db.Ctx.Invoices.AddRange(
            NewInvoice("UNPD0001", "Unpaid", past),                       // should become Overdue
            NewInvoice("PART0002", "Partially Paid", past),               // should become Overdue
            NewInvoice("EXTN0003", "Extension Requested", past),          // should become Overdue
            NewInvoice("PAID0004", "Paid", past),                         // left alone
            NewInvoice("CNCL0005", "Cancelled", past),                    // left alone
            NewInvoice("FUTR0006", "Unpaid", DateTime.Today.AddDays(10))  // not due yet
        );
        await _db.Ctx.SaveChangesAsync();

        var changed = await Sut.MarkOverdueInvoicesAsync();

        changed.Should().Be(3);
        StatusOf("UNPD0001").Should().Be("Overdue");
        StatusOf("PART0002").Should().Be("Overdue");
        StatusOf("EXTN0003").Should().Be("Overdue");
        StatusOf("PAID0004").Should().Be("Paid");
        StatusOf("CNCL0005").Should().Be("Cancelled");
        StatusOf("FUTR0006").Should().Be("Unpaid");
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task MarkOverdue_RunTwice_SecondRunChangesNothing()
    {
        _db.Ctx.Invoices.Add(NewInvoice("UNPD0001", "Unpaid", DateTime.Today.AddDays(-5)));
        await _db.Ctx.SaveChangesAsync();

        (await Sut.MarkOverdueInvoicesAsync()).Should().Be(1);
        (await Sut.MarkOverdueInvoicesAsync()).Should().Be(0);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task ExpireQuotes_ChangesOnlyPendingQuotesPastTheirDate()
    {
        Quote Make(string reference, string status, DateTime? validUntil)
        {
            var q = _db.NewQuote((1, 100m));
            q.ReferenceNumber = reference;
            q.QuoteStatus = status;
            q.QuoteDate = DateTime.Today.AddDays(-30);
            q.ValidUntil = validUntil;
            q.TotalAmount = 100m;
            return q;
        }

        _db.Ctx.Quotes.AddRange(
            Make("QPAST001", "Pending", DateTime.Today.AddDays(-1)),   // should expire
            Make("QFUTR002", "Pending", DateTime.Today.AddDays(5)),    // still valid
            Make("QNONE003", "Pending", null),                         // no expiry date
            Make("QACPT004", "Accepted", DateTime.Today.AddDays(-1))   // already accepted
        );
        await _db.Ctx.SaveChangesAsync();

        var changed = await Sut.ExpireOutdatedQuotesAsync();

        string StatusOfQuote(string r) => _db.Ctx.Quotes.AsNoTracking().Single(q => q.ReferenceNumber == r).QuoteStatus;

        changed.Should().Be(1);
        StatusOfQuote("QPAST001").Should().Be("Expired");
        StatusOfQuote("QFUTR002").Should().Be("Pending");
        StatusOfQuote("QNONE003").Should().Be("Pending");
        StatusOfQuote("QACPT004").Should().Be("Accepted");
    }
}
//--------------------------------------------------------------END OF FILE---------------------------------------------------------------