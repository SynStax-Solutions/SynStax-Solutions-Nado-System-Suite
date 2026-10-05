using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Nado_Graphics_MS_Backend.Models;
using Xunit;

public class QuoteServiceTests : IDisposable
{
    private readonly TestDb _db = new();
    public void Dispose() => _db.Dispose();

    //---------------------------------------------------------------------------------
    // Create
    [Fact]
    public async Task Create_Valid_CalculatesLineAndGrandTotals_IgnoringClientValues()
    {
        var q = _db.NewQuote((2, 150m), (1, 0m));        
        q.QuoteItems.First().LineTotal = 1m;              
        q.TotalAmount = 1m;

        var (ok, err) = await _db.Quotes().CreateQuoteAsync(q);

        ok.Should().BeTrue(err);
        q.TotalAmount.Should().Be(400m);                  
        q.QuoteItems.First().LineTotal.Should().Be(300m);
        q.QuoteStatus.Should().Be("Pending");
        q.ReferenceNumber.Should().NotBeNullOrEmpty();
        q.QuoteID.Should().BeGreaterThan(0);
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Create_ClientSuppliedQuoteId_IsReset()
    {
        var q = _db.NewQuote((1, 10m));
        q.QuoteID = 999;
        await _db.Quotes().CreateQuoteAsync(q);
        q.QuoteID.Should().NotBe(999);
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Create_UnknownClient_Fails()
    {
        var q = _db.NewQuote((1, 10m)); q.ClientID = 12345;
        var (ok, err) = await _db.Quotes().CreateQuoteAsync(q);
        ok.Should().BeFalse(); err.Should().Be("Client not found.");
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Create_NoItems_Fails()
    {
        var q = _db.NewQuote();
        (await _db.Quotes().CreateQuoteAsync(q)).Success.Should().BeFalse();
        q.QuoteItems = null!;
        (await _db.Quotes().CreateQuoteAsync(q)).Success.Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Create_NonPositiveQuantity_Fails(int qty)
    {
        var (ok, err) = await _db.Quotes().CreateQuoteAsync(_db.NewQuote((qty, 10m)));
        ok.Should().BeFalse(); err.Should().Be("Quantity must be greater than zero.");
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Create_InactiveOrMissingProduct_Fails()
    {
        var q = _db.NewQuote((1, 10m));
        q.QuoteItems.First().ProductID = _db.InactiveProductId;
        (await _db.Quotes().CreateQuoteAsync(q)).Success.Should().BeFalse();
        q.QuoteItems.First().ProductID = 99999;
        (await _db.Quotes().CreateQuoteAsync(q)).Success.Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Create_InvalidStatusString_Fails()
    {
        var q = _db.NewQuote((1, 10m)); q.QuoteStatus = "Banana";
        (await _db.Quotes().CreateQuoteAsync(q)).Error.Should().Be("Invalid quote status.");
    }

    //---------------------------------------------------------------------------------
    [Fact] // EXPECTED TO FAIL - caller can create a quote directly as Accepted, bypassing the job card
    public async Task Create_CallerCannotCreateQuoteAsAccepted()
    {
        var q = _db.NewQuote((1, 10m)); q.QuoteStatus = "Accepted";
        var (ok, _) = await _db.Quotes().CreateQuoteAsync(q);
        (!ok || q.QuoteStatus == "Pending").Should().BeTrue();
    }

    //---------------------------------------------------------------------------------
    [Fact] // EXPECTED TO FAIL - a negative price silently falls back to the product price
    public async Task Create_NegativeUnitPrice_IsRejected()
    {
        var (ok, _) = await _db.Quotes().CreateQuoteAsync(_db.NewQuote((1, -50m)));
        ok.Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Create_TwoQuotes_GetDifferentReferenceNumbers()
    {
        var a = _db.NewQuote((1, 10m)); var b = _db.NewQuote((1, 10m));
        await _db.Quotes().CreateQuoteAsync(a);
        await _db.Quotes().CreateQuoteAsync(b);
        a.ReferenceNumber.Should().NotBe(b.ReferenceNumber);
    }

    //---------------------------------------------------------------------------------
    // Update
    [Fact]
    public async Task Update_Pending_ReplacesItemsAndRecalculates()
    {
        var svc = _db.Quotes();
        var q = _db.NewQuote((1, 100m)); await svc.CreateQuoteAsync(q);

        var edit = _db.NewQuote((3, 100m), (1, 50m)); edit.QuoteID = q.QuoteID; edit.Notes = "edited";
        (await svc.UpdateQuoteAsync(q.QuoteID, edit)).Should().BeTrue();

        var saved = await svc.GetQuoteAsync(q.QuoteID);
        saved!.TotalAmount.Should().Be(350m);
        saved.QuoteItems.Should().HaveCount(2);
        saved.Notes.Should().Be("edited");
        saved.QuoteStatus.Should().Be("Pending");
        _db.Ctx.QuoteItems.Count().Should().Be(2);
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Update_IdMismatch_ReturnsFalse()
    {
        var svc = _db.Quotes();
        var q = _db.NewQuote((1, 100m)); await svc.CreateQuoteAsync(q);
        var edit = _db.NewQuote((1, 100m)); edit.QuoteID = q.QuoteID + 1;
        (await svc.UpdateQuoteAsync(q.QuoteID, edit)).Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Update_UnknownQuote_ReturnsFalse()
    {
        var edit = _db.NewQuote((1, 100m)); edit.QuoteID = 404;
        (await _db.Quotes().UpdateQuoteAsync(404, edit)).Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Update_UnknownClient_ReturnsFalse()
    {
        var svc = _db.Quotes();
        var q = _db.NewQuote((1, 100m)); await svc.CreateQuoteAsync(q);
        var edit = _db.NewQuote((1, 100m)); edit.QuoteID = q.QuoteID; edit.ClientID = 404;
        (await svc.UpdateQuoteAsync(q.QuoteID, edit)).Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    [Theory]
    [InlineData("Accepted")]
    [InlineData("Rejected")]
    [InlineData("Expired")]
    public async Task Update_NonPending_ReturnsFalse_AndDoesNotChangeData(string status)
    {
        var svc = _db.Quotes();
        var q = _db.NewQuote((1, 100m)); await svc.CreateQuoteAsync(q);
        (await _db.Ctx.Quotes.FindAsync(q.QuoteID))!.QuoteStatus = status; await _db.Ctx.SaveChangesAsync();

        var edit = _db.NewQuote((5, 100m)); edit.QuoteID = q.QuoteID;
        (await svc.UpdateQuoteAsync(q.QuoteID, edit)).Should().BeFalse();
        (await svc.GetQuoteAsync(q.QuoteID))!.TotalAmount.Should().Be(100m);
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Update_InvalidItem_ReturnsFalse_AndKeepsOriginalItems()
    {
        var svc = _db.Quotes();
        var q = _db.NewQuote((1, 100m)); await svc.CreateQuoteAsync(q);
        var edit = _db.NewQuote((0, 100m)); edit.QuoteID = q.QuoteID;
        (await svc.UpdateQuoteAsync(q.QuoteID, edit)).Should().BeFalse();
        (await svc.GetQuoteAsync(q.QuoteID))!.QuoteItems.Should().HaveCount(1);
    }

    //---------------------------------------------------------------------------------
    [Fact] // EXPECTED TO FAIL - a PUT without ValidUntil overwrites the stored date with null
    public async Task Update_WithoutValidUntil_DoesNotWipeStoredDate()
    {
        var svc = _db.Quotes();
        var q = _db.NewQuote((1, 100m)); await svc.CreateQuoteAsync(q);
        var original = (await svc.GetQuoteAsync(q.QuoteID))!.ValidUntil;
        var edit = _db.NewQuote((1, 100m)); edit.QuoteID = q.QuoteID; edit.ValidUntil = null;
        await svc.UpdateQuoteAsync(q.QuoteID, edit);
        (await svc.GetQuoteAsync(q.QuoteID))!.ValidUntil.Should().Be(original);
    }

    //---------------------------------------------------------------------------------
    // Accept,Reject,Expire
    [Fact]
    public async Task Accept_Pending_SetsAccepted_CreatesExactlyOneJobCard_AndNotifiesHub()
    {
        var svc = _db.Quotes();
        var q = _db.NewQuote((1, 100m)); await svc.CreateQuoteAsync(q);

        var (ok, err) = await svc.AcceptQuoteAsync(q.QuoteID);

        ok.Should().BeTrue(err);
        (await svc.GetQuoteAsync(q.QuoteID))!.QuoteStatus.Should().Be("Accepted");
        _db.Ctx.JobCards.Count().Should().Be(1);
        _db.Ctx.JobCards.Single().ReferenceNumber.Should().Be(q.ReferenceNumber + "-JOB");
        _db.Proxy.Verify(p => p.SendCoreAsync("JobCardsUpdated", It.IsAny<object?[]>(), default), Times.Once);
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Accept_Twice_SecondFails_NoDuplicateJobCard()
    {
        var svc = _db.Quotes();
        var q = _db.NewQuote((1, 100m)); await svc.CreateQuoteAsync(q);
        await svc.AcceptQuoteAsync(q.QuoteID);

        var (ok, err) = await svc.AcceptQuoteAsync(q.QuoteID);

        ok.Should().BeFalse(); err.Should().NotBeNullOrEmpty();
        _db.Ctx.JobCards.Count().Should().Be(1);
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Accept_UnknownQuote_Fails()
    {
        var (ok, err) = await _db.Quotes().AcceptQuoteAsync(404);
        ok.Should().BeFalse(); err.Should().Be("Quote not found.");
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Reject_Pending_SetsRejected_AndCannotThenBeAccepted()
    {
        var svc = _db.Quotes();
        var q = _db.NewQuote((1, 100m)); await svc.CreateQuoteAsync(q);

        (await svc.RejectQuoteAsync(q.QuoteID, "Too expensive")).Success.Should().BeTrue();
        (await svc.GetQuoteAsync(q.QuoteID))!.QuoteStatus.Should().Be("Rejected");
        (await svc.AcceptQuoteAsync(q.QuoteID)).Success.Should().BeFalse();
        _db.Ctx.JobCards.Should().BeEmpty();
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Reject_UnknownQuote_Fails()
    {
        var (ok, err) = await _db.Quotes().RejectQuoteAsync(404, "x");
        ok.Should().BeFalse(); err.Should().Be("Quote not found.");
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Expire_Pending_SetsExpired_ThenAcceptAndRejectFail()
    {
        var svc = _db.Quotes();
        var q = _db.NewQuote((1, 100m)); await svc.CreateQuoteAsync(q);
        (await svc.ExpireQuoteAsync(q.QuoteID, "No response")).Success.Should().BeTrue();
        (await svc.GetQuoteAsync(q.QuoteID))!.QuoteStatus.Should().Be("Expired");
        (await svc.AcceptQuoteAsync(q.QuoteID)).Success.Should().BeFalse();
        (await svc.RejectQuoteAsync(q.QuoteID, "x")).Success.Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Accepted_CannotBeRejectedOrExpired()
    {
        var svc = _db.Quotes();
        var q = _db.NewQuote((1, 100m)); await svc.CreateQuoteAsync(q);
        await svc.AcceptQuoteAsync(q.QuoteID);
        (await svc.RejectQuoteAsync(q.QuoteID, "x")).Success.Should().BeFalse();
        (await svc.ExpireQuoteAsync(q.QuoteID, "x")).Success.Should().BeFalse();
        (await svc.GetQuoteAsync(q.QuoteID))!.QuoteStatus.Should().Be("Accepted");
    }

    //---------------------------------------------------------------------------------
    // Delete
    [Fact]
    public async Task Delete_Pending_RemovesQuoteAndItems()
    {
        var svc = _db.Quotes();
        var q = _db.NewQuote((1, 100m)); await svc.CreateQuoteAsync(q);
        (await svc.DeleteQuoteAsync(q.QuoteID)).Should().BeTrue();
        (await svc.GetQuoteAsync(q.QuoteID)).Should().BeNull();
        _db.Ctx.QuoteItems.Should().BeEmpty();
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Delete_Unknown_ReturnsFalse() =>
        (await _db.Quotes().DeleteQuoteAsync(404)).Should().BeFalse();

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Delete_QuoteThatHasInvoice_FailsCleanly()
    {
        var q = _db.NewQuote((1, 100m)); await _db.Quotes().CreateQuoteAsync(q);
        await _db.Quotes().AcceptQuoteAsync(q.QuoteID);
        await _db.Invoices().CreateInvoiceFromQuoteAsync(q.QuoteID);

        var act = () => _db.Quotes().DeleteQuoteAsync(q.QuoteID);
        await act.Should().NotThrowAsync<DbUpdateException>();
    }

    //---------------------------------------------------------------------------------
    [Fact] // EXPECTED TO FAIL - JobCard has no FK to Quote, so deleting the quote leaves an orphaned job card
    public async Task Delete_AcceptedQuote_WithOnlyJobCard_DoesNotLeaveOrphanJobCard()
    {
        var svc = _db.Quotes();
        var q = _db.NewQuote((1, 100m)); await svc.CreateQuoteAsync(q);
        await svc.AcceptQuoteAsync(q.QuoteID);

        await svc.DeleteQuoteAsync(q.QuoteID);

        _db.Ctx.JobCards.Count().Should().Be(0);
    }
}

//---------------------------------------------------------------------------------
// VAT
public class QuoteVatTests
{
    private static Quote Q(params (decimal line, bool vat)[] items) => new()
    {
        QuoteItems = items.Select(i => new QuoteItem { LineTotal = i.line, IsVatIncluded = i.vat }).ToList()
    };

    [Fact] public void Vat_On115Inclusive_Is15() => Q((115m, true)).VatAmount.Should().Be(15.00m);
    [Fact] public void Vat_NotFlagged_IsZero() => Q((115m, false)).VatAmount.Should().Be(0m);
    [Fact] public void Vat_NoItems_IsZero() => Q().VatAmount.Should().Be(0m);

    [Fact]
    public void Vat_MixedItems_OnlyCountsFlagged() =>
        Q((115m, true), (500m, false), (230m, true)).VatAmount.Should().Be(45.00m);

    [Fact]
    public void Vat_IsRoundedPerLine_NotOnTheSum() =>
        Q((10m, true), (10m, true), (10m, true)).VatAmount.Should().Be(3.90m);

    [Fact]
    public void Vat_DoesNotChangeTotalAmount()
    {
        var q = Q((115m, true)); q.TotalAmount = 115m;
        _ = q.VatAmount;
        q.TotalAmount.Should().Be(115m);
    }
}
//-----------------------------------------------------------END OF FILE----------------------------------------------------------