using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Nado_Graphics_MS_Backend.Hubs;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;
using Nado_Graphics_MS_Backend.Services;
using Xunit;

public class JobCardServiceMockTests
{
    private readonly Mock<IJobCardRepository> _jobCards = new();
    private readonly Mock<ISupplierRepository> _suppliers = new();
    private readonly Mock<IQuoteRepository> _quotes = new();
    private readonly Mock<IRepository<Client>> _clients = new();
    private readonly Mock<IHubContext<JobCardHub>> _hub = new();
    //-------------------------------------------------------------------------------
    private JobCardService Sut
    {
        get
        {
            var proxy = new Mock<IClientProxy>();
            var clients = new Mock<IHubClients>();
            clients.Setup(c => c.All).Returns(proxy.Object);
            _hub.Setup(h => h.Clients).Returns(clients.Object);

            return new JobCardService(_jobCards.Object, _suppliers.Object, _quotes.Object, _clients.Object, _hub.Object);
        }
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task UpdateStatus_InvalidStatus_ReturnsFalse_AndDoesNotSave()
    {
        (await Sut.UpdateStatusAsync(1, "Banana")).Should().BeFalse();
        _jobCards.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task UpdateStatus_Completed_SetsCompletedAt_AndOtherStatusClearsIt()
    {
        var job = new JobCard { JobCardID = 1 };
        _jobCards.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        await Sut.UpdateStatusAsync(1, "Completed");
        job.CompletedAt.Should().NotBeNull();

        await Sut.UpdateStatusAsync(1, "In Progress");
        job.CompletedAt.Should().BeNull();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task AssignSupplier_MissingSupplier_ReturnsFalse()
    {
        _jobCards.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new JobCard { JobCardID = 1 });
        _suppliers.Setup(r => r.GetByIdAsync(9)).ReturnsAsync((Supplier?)null);

        (await Sut.AssignSupplierAsync(1, 9)).Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task UpdatePaymentStatus_InvalidValue_ReturnsFalse()
    {
        (await Sut.UpdatePaymentStatusAsync(1, "Maybe")).Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task CreateFromAcceptedQuote_PendingQuote_IsRefused()
    {
        _quotes.Setup(r => r.GetWithItemsAsync(1))
               .ReturnsAsync(new Quote { QuoteID = 1, QuoteStatus = "Pending" });

        var (success, error) = await Sut.CreateFromAcceptedQuoteAsync(1);

        success.Should().BeFalse();
        error.Should().Contain("accepted");
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task CreateFromAcceptedQuote_JobAlreadyExists_DoesNotCreateAnother()
    {
        _quotes.Setup(r => r.GetWithItemsAsync(1))
               .ReturnsAsync(new Quote { QuoteID = 1, QuoteStatus = "Accepted", ReferenceNumber = "ANNA0001" });
        _jobCards.Setup(r => r.ReferenceExistsAsync("ANNA0001-JOB")).ReturnsAsync(true);

        var (success, _) = await Sut.CreateFromAcceptedQuoteAsync(1);

        success.Should().BeTrue();
        _jobCards.Verify(r => r.AddAsync(It.IsAny<JobCard>()), Times.Never);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task CreateFromAcceptedQuote_Valid_AddsJobCard_ButDoesNotSave()
    {
        _quotes.Setup(r => r.GetWithItemsAsync(1)).ReturnsAsync(new Quote
        {
            QuoteID = 1,
            QuoteStatus = "Accepted",
            ReferenceNumber = "ANNA0001",
            ClientID = 5,
            QuoteItems = new List<QuoteItem> { new() { ProductID = 2, Quantity = 3 } }
        });
        _clients.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Client { ClientID = 5, FirstName = "Ann", LastName = "Lee" });

        var (success, _) = await Sut.CreateFromAcceptedQuoteAsync(1);

        success.Should().BeTrue();
        _jobCards.Verify(r => r.AddAsync(It.Is<JobCard>(j =>
            j.ReferenceNumber == "ANNA0001-JOB" && j.JobStatus == "Not Started")), Times.Once);
        _jobCards.Verify(r => r.SaveChangesAsync(), Times.Never);   //the caller saves
    }
}
//--------------------------------------END OF FILE------------------------------------------------