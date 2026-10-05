using FluentAssertions;
using Moq;
using Nado_Graphics_MS_Backend.Models;
using Xunit;

public class JobCardServiceTests : IDisposable
{
    private readonly TestDb _db = new();
    public void Dispose() => _db.Dispose();

    // Helpers
    private async Task<int> SeedJobCard(string status = "Not Started")
    {
        var jc = new JobCard
        {
            ReferenceNumber = "REF-" + Guid.NewGuid().ToString("N")[..10] + "-JOB",
            ClientID = _db.ClientId,
            JobTitle = "Test job",
            JobStatus = status
        };
        _db.Ctx.JobCards.Add(jc);
        await _db.Ctx.SaveChangesAsync();
        return jc.JobCardID;
    }

    //---------------------------------------------------------------------------------
    private async Task<int> SeedSupplier()
    {
        var s = new Supplier { SupplierName = "Print Co" };
        _db.Ctx.Suppliers.Add(s);
        await _db.Ctx.SaveChangesAsync();
        return s.SupplierID;
    }

    //---------------------------------------------------------------------------------
    // Builds a detached copy of a job card, like the body of a PUT request.
    private async Task<JobCard> Detached(int id)
    {
        var c = (await _db.JobCards().GetJobCardAsync(id))!;
        return new JobCard
        {
            JobCardID = c.JobCardID,
            ReferenceNumber = c.ReferenceNumber,
            ClientID = c.ClientID,
            JobTitle = c.JobTitle,
            JobStatus = c.JobStatus,
            DueDate = c.DueDate
        };
    }

    //---------------------------------------------------------------------------------
    // Create from Accepted Quote
    [Fact]
    public async Task CreateFromAcceptedQuote_UnknownQuote_Fails()
    {
        var (ok, err) = await _db.JobCards().CreateFromAcceptedQuoteAsync(404);
        ok.Should().BeFalse();
        err.Should().Be("Quote not found.");
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task CreateFromAcceptedQuote_PendingQuote_Fails_NoJobCard()
    {
        var q = _db.NewQuote((1, 100m));
        await _db.Quotes().CreateQuoteAsync(q);

        var (ok, err) = await _db.JobCards().CreateFromAcceptedQuoteAsync(q.QuoteID);

        ok.Should().BeFalse();
        err.Should().Be("Only an accepted quote can create a job card.");
        _db.Ctx.JobCards.Should().BeEmpty();
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task AcceptQuote_CreatesJobCard_WithExpectedDefaults()
    {
        var q = _db.NewQuote((2, 100m));
        await _db.Quotes().CreateQuoteAsync(q);
        await _db.Quotes().AcceptQuoteAsync(q.QuoteID);

        var jc = _db.Ctx.JobCards.Single();
        jc.ReferenceNumber.Should().Be(q.ReferenceNumber + "-JOB");
        jc.ClientID.Should().Be(_db.ClientId);
        jc.JobTitle.Should().Be("Job for Jane Doe");
        jc.JobStatus.Should().Be("Not Started");
        jc.PaymentStatus.Should().Be("Pending Payment");
        jc.JobDescription.Should().Contain("x 2");
        jc.SupplierID.Should().BeNull();
        jc.CompletedAt.Should().BeNull();
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task CreateFromAcceptedQuote_CalledTwice_IsIdempotent()
    {
        var q = _db.NewQuote((1, 100m));
        await _db.Quotes().CreateQuoteAsync(q);
        await _db.Quotes().AcceptQuoteAsync(q.QuoteID);

        var (ok, _) = await _db.JobCards().CreateFromAcceptedQuoteAsync(q.QuoteID);
        await _db.Ctx.SaveChangesAsync();

        ok.Should().BeTrue();
        _db.Ctx.JobCards.Count().Should().Be(1);
    }

    //---------------------------------------------------------------------------------
    // Create from Invoice
    [Fact]
    public async Task CreateFromInvoice_LongDescription_TruncatedTo500()
    {
        var inv = new Invoice { ReferenceNumber = "INV-1", ClientID = _db.ClientId };
        await _db.JobCards().CreateFromInvoiceAsync(inv, "Jane Doe", new string('x', 600));
        await _db.Ctx.SaveChangesAsync();

        var d = _db.Ctx.JobCards.Single().JobDescription!;
        d.Length.Should().Be(500);
        d.Should().EndWith("...");
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task CreateFromInvoice_NullDescription_FallsBackToInvoiceNotes()
    {
        var inv = new Invoice { ReferenceNumber = "INV-2", ClientID = _db.ClientId, Notes = "Rush job" };
        await _db.JobCards().CreateFromInvoiceAsync(inv, "Jane Doe", null);
        await _db.Ctx.SaveChangesAsync();

        _db.Ctx.JobCards.Single().JobDescription.Should().Be("Rush job");
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task CreateFromInvoice_SetsReferencePlusJob_AndDefaults()
    {
        var inv = new Invoice { ReferenceNumber = "INV-4", ClientID = _db.ClientId };
        await _db.JobCards().CreateFromInvoiceAsync(inv, "Jane Doe", "Banner x 1");
        await _db.Ctx.SaveChangesAsync();

        var jc = _db.Ctx.JobCards.Single();
        jc.ReferenceNumber.Should().Be("INV-4-JOB");
        jc.JobTitle.Should().Be("Job for Jane Doe");
        jc.JobStatus.Should().Be("Not Started");
        jc.PaymentStatus.Should().Be("Pending Payment");
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task CreateFromInvoice_ExistingReference_AddsNothing()
    {
        var inv = new Invoice { ReferenceNumber = "INV-3", ClientID = _db.ClientId };
        await _db.JobCards().CreateFromInvoiceAsync(inv, "Jane Doe", "a");
        await _db.Ctx.SaveChangesAsync();
        await _db.JobCards().CreateFromInvoiceAsync(inv, "Jane Doe", "b");
        await _db.Ctx.SaveChangesAsync();

        _db.Ctx.JobCards.Count().Should().Be(1);
    }

    //---------------------------------------------------------------------------------
    // GET
    [Fact]
    public async Task GetJobCard_Existing_ReturnsIt_UnknownReturnsNull()
    {
        var id = await SeedJobCard();
        (await _db.JobCards().GetJobCardAsync(id))!.JobCardID.Should().Be(id);
        (await _db.JobCards().GetJobCardAsync(404)).Should().BeNull();
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task GetJobCards_ExcludesArchived_ArchivedListContainsOnlyArchived()
    {
        var active = await SeedJobCard("In Progress");
        var archived = await SeedJobCard("Archived");

        (await _db.JobCards().GetJobCardsAsync()).Select(j => j.JobCardID)
            .Should().Contain(active).And.NotContain(archived);
        (await _db.JobCards().GetArchivedJobCardsAsync()).Select(j => j.JobCardID)
            .Should().Contain(archived).And.NotContain(active);
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task GetJobCards_AreOrderedNewestFirst()
    {
        var older = new JobCard { ReferenceNumber = "OLD-JOB", ClientID = _db.ClientId, JobTitle = "Old", JobStatus = "Not Started", JobDate = DateTime.Now.AddDays(-5) };
        var newer = new JobCard { ReferenceNumber = "NEW-JOB", ClientID = _db.ClientId, JobTitle = "New", JobStatus = "Not Started", JobDate = DateTime.Now };
        _db.Ctx.JobCards.AddRange(older, newer);
        await _db.Ctx.SaveChangesAsync();

        var list = await _db.JobCards().GetJobCardsAsync();
        list.First().ReferenceNumber.Should().Be("NEW-JOB");
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task GetSuppliers_ReturnsAlphabetically()
    {
        _db.Ctx.Suppliers.AddRange(new Supplier { SupplierName = "Zeta" }, new Supplier { SupplierName = "Alpha" });
        await _db.Ctx.SaveChangesAsync();

        var names = (await _db.JobCards().GetSuppliersAsync()).Select(s => s.SupplierName).ToList();
        names.Should().BeInAscendingOrder();
    }

    //---------------------------------------------------------------------------------
    // Status
    [Theory]
    [InlineData("Not Started")]
    [InlineData("In Progress")]
    [InlineData("Completed")]
    [InlineData("Archived")]
    public async Task UpdateStatus_AllowedValues_Succeed_AndNotifyHub(string status)
    {
        var id = await SeedJobCard("Not Started");

        (await _db.JobCards().UpdateStatusAsync(id, status)).Should().BeTrue();

        (await _db.JobCards().GetJobCardAsync(id))!.JobStatus.Should().Be(status);
        _db.Proxy.Verify(p => p.SendCoreAsync("JobCardsUpdated", It.IsAny<object?[]>(), default), Times.Once);
    }

    //---------------------------------------------------------------------------------
    [Theory]
    [InlineData("Banana")]
    [InlineData("completed")]
    [InlineData(" Completed")]
    [InlineData("")]
    [InlineData(null)]
    public async Task UpdateStatus_InvalidValues_Fail_AndStatusUnchanged(string? status)
    {
        var id = await SeedJobCard("In Progress");

        (await _db.JobCards().UpdateStatusAsync(id, status!)).Should().BeFalse();

        (await _db.JobCards().GetJobCardAsync(id))!.JobStatus.Should().Be("In Progress");
        _db.Proxy.Verify(p => p.SendCoreAsync("JobCardsUpdated", It.IsAny<object?[]>(), default), Times.Never);
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task UpdateStatus_UnknownJobCard_Fails() =>
        (await _db.JobCards().UpdateStatusAsync(404, "Completed")).Should().BeFalse();

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task UpdateStatus_Completed_SetsCompletedAt_AndLeavingClearsIt()
    {
        var id = await SeedJobCard();

        await _db.JobCards().UpdateStatusAsync(id, "Completed");
        var done = (await _db.JobCards().GetJobCardAsync(id))!.CompletedAt;
        done.Should().NotBeNull();
        done!.Value.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));

        await _db.JobCards().UpdateStatusAsync(id, "In Progress");
        (await _db.JobCards().GetJobCardAsync(id))!.CompletedAt.Should().BeNull();
    }

    //---------------------------------------------------------------------------------
    [Fact] // tests that there are no transition rules
    public async Task UpdateStatus_CompletedBackToNotStarted_IsCurrentlyAllowed()
    {
        var id = await SeedJobCard("Completed");
        (await _db.JobCards().UpdateStatusAsync(id, "Not Started")).Should().BeTrue();
    }

    //---------------------------------------------------------------------------------
    [Fact] // tests that archiving also clears CompletedAt
    public async Task UpdateStatus_CompletedThenArchived_ClearsCompletedAt()
    {
        var id = await SeedJobCard();
        await _db.JobCards().UpdateStatusAsync(id, "Completed");
        await _db.JobCards().UpdateStatusAsync(id, "Archived");

        (await _db.JobCards().GetJobCardAsync(id))!.CompletedAt.Should().BeNull();
    }

    //---------------------------------------------------------------------------------
    // Payment Status
    [Theory]
    [InlineData("Paid")]
    [InlineData("Pending Payment")]
    public async Task UpdatePaymentStatus_Allowed_Succeeds(string value)
    {
        var id = await SeedJobCard();
        (await _db.JobCards().UpdatePaymentStatusAsync(id, value)).Should().BeTrue();
        (await _db.JobCards().GetJobCardAsync(id))!.PaymentStatus.Should().Be(value);
    }

    //---------------------------------------------------------------------------------
    [Theory]
    [InlineData("paid")]
    [InlineData("Refunded")]
    [InlineData("")]
    [InlineData("Partially Paid")]
    public async Task UpdatePaymentStatus_Invalid_Fails_AndUnchanged(string value)
    {
        var id = await SeedJobCard();
        (await _db.JobCards().UpdatePaymentStatusAsync(id, value)).Should().BeFalse();
        (await _db.JobCards().GetJobCardAsync(id))!.PaymentStatus.Should().Be("Pending Payment");
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task UpdatePaymentStatus_UnknownJobCard_Fails() =>
        (await _db.JobCards().UpdatePaymentStatusAsync(404, "Paid")).Should().BeFalse();

    //---------------------------------------------------------------------------------
    // Supplier
    [Fact]
    public async Task AssignSupplier_Valid_SetsSupplier_AndReassignWorks()
    {
        var jc = await SeedJobCard();
        var s1 = await SeedSupplier();
        var s2 = await SeedSupplier();

        (await _db.JobCards().AssignSupplierAsync(jc, s1)).Should().BeTrue();
        (await _db.JobCards().AssignSupplierAsync(jc, s2)).Should().BeTrue();

        (await _db.JobCards().GetJobCardAsync(jc))!.SupplierID.Should().Be(s2);
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task AssignSupplier_UnknownSupplierOrJobCard_Fails()
    {
        var jc = await SeedJobCard();

        (await _db.JobCards().AssignSupplierAsync(jc, 404)).Should().BeFalse();
        (await _db.JobCards().AssignSupplierAsync(404, await SeedSupplier())).Should().BeFalse();
        (await _db.JobCards().GetJobCardAsync(jc))!.SupplierID.Should().BeNull();
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task AssignSupplier_NotifiesHub()
    {
        var jc = await SeedJobCard();
        var s = await SeedSupplier();
        await _db.JobCards().AssignSupplierAsync(jc, s);
        _db.Proxy.Verify(p => p.SendCoreAsync("JobCardsUpdated", It.IsAny<object?[]>(), default), Times.Once);
    }

    //---------------------------------------------------------------------------------
    // Update (PUT {id})
    [Fact]
    public async Task Update_IdMismatch_Fails()
    {
        var id = await SeedJobCard();
        var edit = await Detached(id);
        (await _db.JobCards().UpdateJobCardAsync(id + 1, edit)).Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Update_UnknownJobCard_Fails()
    {
        var id = await SeedJobCard();
        var edit = await Detached(id);
        edit.JobCardID = 404;
        (await _db.JobCards().UpdateJobCardAsync(404, edit)).Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    [Fact]
    public async Task Update_ChangesTitleAndDueDate_ButNeverSupplierOrPaymentStatus()
    {
        var id = await SeedJobCard();
        var sup = await SeedSupplier();
        await _db.JobCards().AssignSupplierAsync(id, sup);

        var edit = await Detached(id);
        edit.JobTitle = "New title";
        edit.DueDate = DateTime.Today.AddDays(7);
        edit.SupplierID = null;               
        edit.PaymentStatus = "Paid";

        (await _db.JobCards().UpdateJobCardAsync(id, edit)).Should().BeTrue();

        var saved = (await _db.JobCards().GetJobCardAsync(id))!;
        saved.JobTitle.Should().Be("New title");
        saved.DueDate.Should().Be(DateTime.Today.AddDays(7));
        saved.SupplierID.Should().Be(sup);
        saved.PaymentStatus.Should().Be("Pending Payment");
    }

    //---------------------------------------------------------------------------------
    [Fact] // EXPECTED TO FAIL - PUT bypasses the allowed-status list used by /status
    public async Task Update_InvalidJobStatus_IsRejected()
    {
        var id = await SeedJobCard();
        var edit = await Detached(id);
        edit.JobStatus = "Banana";
        (await _db.JobCards().UpdateJobCardAsync(id, edit)).Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    [Fact] // EXPECTED TO FAIL - reference number can be rewritten, breaking the quote/invoice link
    public async Task Update_CannotChangeReferenceNumber()
    {
        var id = await SeedJobCard();
        var original = (await _db.JobCards().GetJobCardAsync(id))!.ReferenceNumber;
        var edit = await Detached(id);
        edit.ReferenceNumber = "HACKED-JOB";

        await _db.JobCards().UpdateJobCardAsync(id, edit);

        (await _db.JobCards().GetJobCardAsync(id))!.ReferenceNumber.Should().Be(original);
    }

    //---------------------------------------------------------------------------------
    [Fact] // EXPECTED TO FAIL - CompletedAt can be set manually without the Completed status
    public async Task Update_CannotSetCompletedAtManually()
    {
        var id = await SeedJobCard();
        var edit = await Detached(id);
        edit.CompletedAt = DateTime.Now;

        await _db.JobCards().UpdateJobCardAsync(id, edit);

        (await _db.JobCards().GetJobCardAsync(id))!.CompletedAt.Should().BeNull();
    }

    //---------------------------------------------------------------------------------
    [Fact] // EXPECTED TO FAIL - ClientID can be repointed to any number (no FK on JobCard.ClientID)
    public async Task Update_CannotChangeClientToNonExistentClient()
    {
        var id = await SeedJobCard();
        var edit = await Detached(id);
        edit.ClientID = 99999;

        (await _db.JobCards().UpdateJobCardAsync(id, edit)).Should().BeFalse();
    }

    //---------------------------------------------------------------------------------
    // Delete
    [Fact]
    public async Task Delete_Existing_RemovesIt_UnknownReturnsFalse()
    {
        var id = await SeedJobCard();
        (await _db.JobCards().DeleteJobCardAsync(id)).Should().BeTrue();
        (await _db.JobCards().GetJobCardAsync(id)).Should().BeNull();
        (await _db.JobCards().DeleteJobCardAsync(404)).Should().BeFalse();
    }
    //---------------------------------------------------------------------------------
}