using FluentAssertions;
using Moq;
using Nado_Graphics_MS_Backend.Models;
using Xunit;
using static Nado_Graphics_MS_Backend.Models.PaymentMethodOptions;

public class InvoiceServiceTests : IDisposable
{
    private readonly TestDb _db = new();
    public void Dispose() => _db.Dispose();

    //---------------------------------------------------------------------------------
    private async Task<int> AcceptedQuote(params (int qty, decimal price)[] lines)
    {
        var q = _db.NewQuote(lines.Length == 0 ? new[] { (1, 100m) } : lines);
        await _db.Quotes().CreateQuoteAsync(q);
        await _db.Quotes().AcceptQuoteAsync(q.QuoteID);
        return q.QuoteID;
    }

    //---------------------------------------------------------------------------------
    private async Task<Invoice> UnpaidInvoice(decimal price = 1000m)
    {
        var (ok, err, inv) = await _db.Invoices().CreateInvoiceManuallyAsync(new CreateManualInvoiceRequest  
        {
            ClientID = _db.ClientId,
            InvoiceItems = new() { new ManualInvoiceItemRequest { ProductID = _db.ProductId, Quantity = 1, UnitPrice = price } }
        });
        ok.Should().BeTrue(err);
        return inv!;
    }

    //---------------------------------------------------------------------------------
    private static RecordPaymentRequest Pay(decimal amt) =>
        new() { Amount = amt, PaymentMethod = PaymentMethod.EFT, TransactionReference = "T1" };

    //---------------------------------------------------------------------------------
    // QUOTE 
    [Fact]
    public async Task FromQuote_Accepted_CopiesItemsAndTotal_Unpaid_Due30Days()
    {
        var qid = await AcceptedQuote((2, 150m), (1, 100m));

        var (ok, err, inv) = await _db.Invoices().CreateInvoiceFromQuoteAsync(qid);

        ok.Should().BeTrue(err);
        inv!.TotalAmount.Should().Be(400m);
        inv.AmountPaid.Should().Be(0m);
        inv.InvoiceItems.Should().HaveCount(2);
        inv.QuoteID.Should().Be(qid);
        inv.InvoiceStatus.Should().Be(States.InvoiceStatus.Unpaid.ToDbString());
        inv.DueDate.Date.Should().Be(DateTime.Now.AddDays(30).Date);
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task FromQuote_Pending_Fails()
    {
        var q = _db.NewQuote((1, 100m)); await _db.Quotes().CreateQuoteAsync(q);
        var (ok, err, _) = await _db.Invoices().CreateInvoiceFromQuoteAsync(q.QuoteID);
        ok.Should().BeFalse(); err.Should().Be("Can only create an invoice from an accepted quote.");
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task FromQuote_UnknownQuote_Fails() =>
        (await _db.Invoices().CreateInvoiceFromQuoteAsync(404)).Error.Should().Be("Quote not found.");

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task FromQuote_Twice_SecondFails()
    {
        var qid = await AcceptedQuote();
        await _db.Invoices().CreateInvoiceFromQuoteAsync(qid);
        var (ok, err, _) = await _db.Invoices().CreateInvoiceFromQuoteAsync(qid);
        ok.Should().BeFalse(); err.Should().Be("An invoice already exists for this quote.");
    }

    //---------------------------------------------------------------------------------
    [Fact] // accept already made the job card so invoicing must not make another one
    public async Task QuoteAcceptThenInvoice_ResultsInExactlyOneJobCard()
    {
        var qid = await AcceptedQuote();
        await _db.Invoices().CreateInvoiceFromQuoteAsync(qid);
        _db.Ctx.JobCards.Count().Should().Be(1);
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task QuoteAcceptThenInvoice_NoDuplicateJobCardsForSameReference()
    {
        var qid = await AcceptedQuote();
        await _db.Invoices().CreateInvoiceFromQuoteAsync(qid);
        _db.Ctx.JobCards.GroupBy(j => j.ReferenceNumber).Should().OnlyContain(g => g.Count() == 1);
    }

    //---------------------------------------------------------------------------------
    [Fact] // Quote, invoice and job share one reference number (job card adds "-JOB")
    public async Task FromQuote_InvoiceSharesQuoteReference_JobCardIsReferencePlusJob()
    {
        var qid = await AcceptedQuote();
        var (_, _, inv) = await _db.Invoices().CreateInvoiceFromQuoteAsync(qid);
        var quote = await _db.Quotes().GetQuoteAsync(qid);

        inv!.ReferenceNumber.Should().Be(quote!.ReferenceNumber);
        _db.Ctx.JobCards.Should().ContainSingle(j => j.ReferenceNumber == quote.ReferenceNumber + "-JOB");
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task FromQuote_InactiveProductSinceQuoting_Fails()
    {
        var qid = await AcceptedQuote();
        (await _db.Ctx.Products.FindAsync(_db.ProductId))!.IsActive = false; await _db.Ctx.SaveChangesAsync();
        (await _db.Invoices().CreateInvoiceFromQuoteAsync(qid)).Success.Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    // Manual Invoices
    [Fact]
    public async Task Manual_Valid_CreatesInvoiceAndOneJobCard_AndNotifies()
    {
        var inv = await UnpaidInvoice(250m);
        inv.TotalAmount.Should().Be(250m);
        inv.QuoteID.Should().BeNull();
        _db.Ctx.JobCards.Count().Should().Be(1);
        _db.Ctx.JobCards.Single().ReferenceNumber.Should().Be(inv.ReferenceNumber + "-JOB");
        _db.Ctx.JobCards.Single().ClientID.Should().Be(inv.ClientID);
        _db.Proxy.Verify(p => p.SendCoreAsync("JobCardsUpdated", It.IsAny<object?[]>(), default), Times.AtLeastOnce);
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Manual_NullUnitPrice_UsesProductPrice()
    {
        var (_, _, inv) = await _db.Invoices().CreateInvoiceManuallyAsync(new CreateManualInvoiceRequest
        { ClientID = _db.ClientId, InvoiceItems = new() { new() { ProductID = _db.ProductId, Quantity = 3, UnitPrice = null } } });
        inv!.TotalAmount.Should().Be(300m);
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Manual_UnknownClient_NoItems_BadProduct_BadQuantity_AllFail_AndCreateNothing()
    {
        var svc = _db.Invoices();
        (await svc.CreateInvoiceManuallyAsync(new() { ClientID = 404, InvoiceItems = new() { new() { ProductID = _db.ProductId, Quantity = 1 } } })).Success.Should().BeFalse();
        (await svc.CreateInvoiceManuallyAsync(new() { ClientID = _db.ClientId, InvoiceItems = new() })).Success.Should().BeFalse();
        (await svc.CreateInvoiceManuallyAsync(new() { ClientID = _db.ClientId, InvoiceItems = new() { new() { ProductID = 404, Quantity = 1 } } })).Success.Should().BeFalse();
        (await svc.CreateInvoiceManuallyAsync(new() { ClientID = _db.ClientId, InvoiceItems = new() { new() { ProductID = _db.ProductId, Quantity = 0 } } })).Success.Should().BeFalse();
        _db.Ctx.Invoices.Should().BeEmpty();
        _db.Ctx.JobCards.Should().BeEmpty();
    }

    //---------------------------------------------------------------------------------
    // Payments
    [Fact]
    public async Task Payment_Partial_UpdatesBalanceAndStatus_AndWritesPaymentRow()
    {
        var inv = await UnpaidInvoice(1000m);
        var (ok, err) = await _db.Invoices().RecordPaymentAsync(inv.InvoiceID, Pay(400m));

        ok.Should().BeTrue(err);
        var saved = await _db.Invoices().GetInvoiceAsync(inv.InvoiceID);
        saved!.AmountPaid.Should().Be(400m);
        saved.InvoiceStatus.Should().Be(States.InvoiceStatus.PartiallyPaid.ToDbString());
        var p = _db.Ctx.Payments.Single();
        p.Amount.Should().Be(400m);
        p.InvoiceID.Should().Be(inv.InvoiceID);
        p.ClientID.Should().Be(inv.ClientID);
        p.PaymentMethod.Should().Be(PaymentMethod.EFT);
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Payment_Full_SetsPaid()
    {
        var inv = await UnpaidInvoice(1000m);
        await _db.Invoices().RecordPaymentAsync(inv.InvoiceID, Pay(1000m));
        var saved = await _db.Invoices().GetInvoiceAsync(inv.InvoiceID);
        saved!.InvoiceStatus.Should().Be(States.InvoiceStatus.Paid.ToDbString());
        saved.AmountPaid.Should().Be(saved.TotalAmount);
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Payment_MultiplePartials_SumToExactlyTotal_AndBecomePaid()
    {
        var inv = await UnpaidInvoice(100m);
        foreach (var amt in new[] { 33.33m, 33.33m, 33.34m })
            (await _db.Invoices().RecordPaymentAsync(inv.InvoiceID, Pay(amt))).Success.Should().BeTrue();
        var saved = await _db.Invoices().GetInvoiceAsync(inv.InvoiceID);
        saved!.AmountPaid.Should().Be(100m);
        saved.InvoiceStatus.Should().Be(States.InvoiceStatus.Paid.ToDbString());
        _db.Ctx.Payments.Count().Should().Be(3);
    }

    //---------------------------------------------------------------------------------
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Payment_ZeroOrNegative_Fails_NoPaymentRow(decimal amt)
    {
        var inv = await UnpaidInvoice();
        var (ok, err) = await _db.Invoices().RecordPaymentAsync(inv.InvoiceID, Pay(amt));
        ok.Should().BeFalse(); err.Should().Be("Payment amount must be greater than zero.");
        _db.Ctx.Payments.Should().BeEmpty();
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Payment_Overpayment_Fails_AndPaidUnchanged()
    {
        var inv = await UnpaidInvoice(1000m);
        var (ok, err) = await _db.Invoices().RecordPaymentAsync(inv.InvoiceID, Pay(1000.01m));
        ok.Should().BeFalse(); err.Should().Be("Payment exceeds the outstanding balance.");
        (await _db.Invoices().GetInvoiceAsync(inv.InvoiceID))!.AmountPaid.Should().Be(0m);
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Payment_OnPaidInvoice_Fails()
    {
        var inv = await UnpaidInvoice(100m);
        await _db.Invoices().RecordPaymentAsync(inv.InvoiceID, Pay(100m));
        (await _db.Invoices().RecordPaymentAsync(inv.InvoiceID, Pay(1m))).Success.Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Payment_OnCancelledInvoice_Fails_NoPaymentRow()
    {
        var inv = await UnpaidInvoice();
        await _db.Invoices().CancelInvoiceAsync(inv.InvoiceID, "Client withdrew");
        var (ok, _) = await _db.Invoices().RecordPaymentAsync(inv.InvoiceID, Pay(10m));
        ok.Should().BeFalse();
        _db.Ctx.Payments.Should().BeEmpty();
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Payment_UnknownInvoice_Fails() =>
        (await _db.Invoices().RecordPaymentAsync(404, Pay(10m))).Error.Should().Be("Invoice was not found.");

    //---------------------------------------------------------------------------------
    [Fact] // EXPECTED TO FAIL, amounts with more than 2 decimals are not rejected
    public async Task Payment_MoreThanTwoDecimals_IsRejected()
    {
        var inv = await UnpaidInvoice();
        (await _db.Invoices().RecordPaymentAsync(inv.InvoiceID, Pay(10.005m))).Success.Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    [Fact] // tests that payments never touch the job card
    public async Task Payment_Full_DoesNotChangeJobCardPaymentStatus_ConfirmIntended()
    {
        var inv = await UnpaidInvoice(100m);
        var before = _db.Ctx.JobCards.Single().PaymentStatus;
        await _db.Invoices().RecordPaymentAsync(inv.InvoiceID, Pay(100m));
        _db.Ctx.Entry(_db.Ctx.JobCards.Single()).Reload();
        _db.Ctx.JobCards.Single().PaymentStatus.Should().Be(before);
    }

    //---------------------------------------------------------------------------------
    // Update
    [Fact]
    public async Task Update_Unpaid_ReplacesItemsAndRecalculates()
    {
        var inv = await UnpaidInvoice(100m);
        var edit = new Invoice
        {
            InvoiceID = inv.InvoiceID,
            ClientID = _db.ClientId,
            DueDate = inv.DueDate.AddDays(5),
            Notes = "n",
            InvoiceItems = new() { new InvoiceItem { ProductID = _db.ProductId, Quantity = 4, UnitPrice = 50m } }
        };
        (await _db.Invoices().UpdateInvoiceAsync(inv.InvoiceID, edit)).Should().BeTrue();
        var saved = await _db.Invoices().GetInvoiceAsync(inv.InvoiceID);
        saved!.TotalAmount.Should().Be(200m);
        saved.InvoiceItems.Should().HaveCount(1);
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Update_TotalBelowAmountPaid_Fails()
    {
        var inv = await UnpaidInvoice(1000m);
        await _db.Invoices().RecordPaymentAsync(inv.InvoiceID, Pay(600m));
        var edit = new Invoice
        {
            InvoiceID = inv.InvoiceID,
            ClientID = _db.ClientId,
            DueDate = inv.DueDate,
            InvoiceItems = new() { new InvoiceItem { ProductID = _db.ProductId, Quantity = 1, UnitPrice = 500m } }
        };
        (await _db.Invoices().UpdateInvoiceAsync(inv.InvoiceID, edit)).Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    [Fact] // EXPECTED TO FAIL - total equal to AmountPaid leaves the status as PartiallyPaid
    public async Task Update_TotalEqualToAmountPaid_StatusBecomesPaid()
    {
        var inv = await UnpaidInvoice(1000m);
        await _db.Invoices().RecordPaymentAsync(inv.InvoiceID, Pay(600m));
        var edit = new Invoice
        {
            InvoiceID = inv.InvoiceID,
            ClientID = _db.ClientId,
            DueDate = inv.DueDate,
            InvoiceItems = new() { new InvoiceItem { ProductID = _db.ProductId, Quantity = 1, UnitPrice = 600m } }
        };
        await _db.Invoices().UpdateInvoiceAsync(inv.InvoiceID, edit);
        (await _db.Invoices().GetInvoiceAsync(inv.InvoiceID))!.InvoiceStatus.Should().Be(States.InvoiceStatus.Paid.ToDbString());
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Update_PaidOrCancelled_Fails()
    {
        var paid = await UnpaidInvoice(100m);
        await _db.Invoices().RecordPaymentAsync(paid.InvoiceID, Pay(100m));
        var cancelled = await UnpaidInvoice(100m);
        await _db.Invoices().CancelInvoiceAsync(cancelled.InvoiceID, "x");
        foreach (var id in new[] { paid.InvoiceID, cancelled.InvoiceID })
        {
            var edit = new Invoice
            {
                InvoiceID = id,
                ClientID = _db.ClientId,
                DueDate = DateTime.Now,
                InvoiceItems = new() { new InvoiceItem { ProductID = _db.ProductId, Quantity = 1, UnitPrice = 100m } }
            };
            (await _db.Invoices().UpdateInvoiceAsync(id, edit)).Should().BeFalse();
        }
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Update_IdMismatch_UnknownClient_NoItems_AllFail()
    {
        var inv = await UnpaidInvoice();
        var good = () => new Invoice
        {
            InvoiceID = inv.InvoiceID,
            ClientID = _db.ClientId,
            DueDate = inv.DueDate,
            InvoiceItems = new() { new InvoiceItem { ProductID = _db.ProductId, Quantity = 1, UnitPrice = 10m } }
        };
        var a = good(); a.InvoiceID = 999; (await _db.Invoices().UpdateInvoiceAsync(inv.InvoiceID, a)).Should().BeFalse();
        var b = good(); b.ClientID = 404; (await _db.Invoices().UpdateInvoiceAsync(inv.InvoiceID, b)).Should().BeFalse();
        var c = good(); c.InvoiceItems = new(); (await _db.Invoices().UpdateInvoiceAsync(inv.InvoiceID, c)).Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    // Payment Extension Test
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task Extension_NoReason_Fails(string? reason)
    {
        var inv = await UnpaidInvoice();
        var (ok, err) = await _db.Invoices().ExtensionInvoiceAsync(inv.InvoiceID, reason!);
        ok.Should().BeFalse(); err.Should().Be("A reason is required for an extension.");
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Extension_OnOverdue_Adds30DaysToOldDueDate_AndChangesStatus()
    {
        var inv = await UnpaidInvoice();
        var tracked = await _db.Ctx.Invoices.FindAsync(inv.InvoiceID);
        tracked!.DueDate = DateTime.Now.AddDays(-5);
        tracked.InvoiceStatus = States.InvoiceStatus.Overdue.ToDbString();
        await _db.Ctx.SaveChangesAsync();
        var oldDue = tracked.DueDate;

        var (ok, err) = await _db.Invoices().ExtensionInvoiceAsync(inv.InvoiceID, "Client cash-flow");

        ok.Should().BeTrue(err);
        (await _db.Invoices().GetInvoiceAsync(inv.InvoiceID))!.DueDate.Should().Be(oldDue.AddDays(30));
    }

    //---------------------------------------------------------------------------------
    [Fact] // EXPECTED TO FAIL
    public async Task Extension_OnLongOverdue_NewDueDateIsInTheFuture()
    {
        var inv = await UnpaidInvoice();
        var t = await _db.Ctx.Invoices.FindAsync(inv.InvoiceID);
        t!.DueDate = DateTime.Now.AddDays(-90); t.InvoiceStatus = States.InvoiceStatus.Overdue.ToDbString();
        await _db.Ctx.SaveChangesAsync();
        await _db.Invoices().ExtensionInvoiceAsync(inv.InvoiceID, "reason");
        (await _db.Invoices().GetInvoiceAsync(inv.InvoiceID))!.DueDate.Should().BeAfter(DateTime.Now);
    }

    //---------------------------------------------------------------------------------
    // Cancel Invoice
    [Fact]
    public async Task Cancel_Unpaid_WithReason_SetsCancelled()
    {
        var inv = await UnpaidInvoice();
        (await _db.Invoices().CancelInvoiceAsync(inv.InvoiceID, "Duplicate")).Success.Should().BeTrue();
        (await _db.Invoices().GetInvoiceAsync(inv.InvoiceID))!.InvoiceStatus.Should().Be(States.InvoiceStatus.Cancelled.ToDbString());
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Cancel_NoReason_UnknownInvoice_AlreadyCancelled_AllFail()
    {
        var inv = await UnpaidInvoice();
        (await _db.Invoices().CancelInvoiceAsync(inv.InvoiceID, "")).Error.Should().Be("A reason is required to cancel an invoice.");
        (await _db.Invoices().CancelInvoiceAsync(404, "x")).Error.Should().Be("Invoice not found.");
        await _db.Invoices().CancelInvoiceAsync(inv.InvoiceID, "x");
        (await _db.Invoices().CancelInvoiceAsync(inv.InvoiceID, "again")).Success.Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Cancel_PartiallyPaidAndPaid_BehaviourIsDefined()
    {
        var inv = await UnpaidInvoice(100m);
        await _db.Invoices().RecordPaymentAsync(inv.InvoiceID, Pay(50m));
        var (ok, _) = await _db.Invoices().CancelInvoiceAsync(inv.InvoiceID, "x");
        ok.Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    // Delete Invoice
    [Fact]
    public async Task Delete_Unknown_ReturnsFalse() =>
        (await _db.Invoices().DeleteInvoiceAsync(404)).Should().BeFalse();

    //---------------------------------------------------------------------------------
    [Fact] // EXPECTED TO FAIL - Payment to Invoice is restrict, so this throws DbUpdateException
    public async Task Delete_InvoiceWithPayments_IsBlockedCleanly()
    {
        var inv = await UnpaidInvoice(100m);
        await _db.Invoices().RecordPaymentAsync(inv.InvoiceID, Pay(50m));
        var result = false;
        var act = async () => result = await _db.Invoices().DeleteInvoiceAsync(inv.InvoiceID);
        await act.Should().NotThrowAsync();
        result.Should().BeFalse();
    }
}
//--------------------------------------------------------------END OF FILE-----------------------------------------------------------