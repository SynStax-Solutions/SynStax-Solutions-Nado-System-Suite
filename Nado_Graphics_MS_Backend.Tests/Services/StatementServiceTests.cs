using FluentAssertions;
using Moq;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;
using Nado_Graphics_MS_Backend.Services;
using Xunit;

public class StatementServiceTests
{
    private readonly Mock<IStatementRepository> _statements = new();
    private readonly Mock<IClientRepository> _clients = new();
    private readonly Mock<IOrganisationRepository> _orgs = new();
    private readonly Mock<IInvoiceRepository> _invoices = new();

    private StatementService Sut => new(_statements.Object, _clients.Object, _orgs.Object, _invoices.Object);
    //-------------------------------------------------------------------------------
    private void ClientExists()
    {
        _clients.Setup(c => c.GetByIdAsync(1)).ReturnsAsync(new Client
        {
            ClientID = 1,
            FirstName = "Ann",
            LastName = "Lee",
            City = "Cape Town",
            PostalCode = "8001"
        });
        _clients.Setup(c => c.GetFullNamesByIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new Dictionary<int, string> { { 1, "Ann Lee" } });
    }
    //-------------------------------------------------------------------------------
    private void Activity(List<Invoice> invoices, List<Payment> payments)
    {
        _invoices.Setup(i => i.GetForStatementAsync(It.IsAny<List<int>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                 .ReturnsAsync(invoices);
        _invoices.Setup(i => i.GetPaymentsForInvoicesAsync(It.IsAny<List<int>>(), It.IsAny<DateTime>()))
                 .ReturnsAsync(payments);
    }
    //-------------------------------------------------------------------------------
    //Create validation

    [Fact]
    public async Task Create_WithBothClientAndOrganisation_Fails()
    {
        var (ok, error, _) = await Sut.CreateStatementAsync(new CreateStatementRequest
        {
            ClientID = 1,
            OrganisationID = 2,
            StartDate = new DateTime(2026, 3, 1),
            EndDate = new DateTime(2026, 3, 31)
        });

        ok.Should().BeFalse();
        error.Should().Contain("either a ClientID or an OrganisationID");
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_WithNeitherClientNorOrganisation_Fails()
    {
        var (ok, _, _) = await Sut.CreateStatementAsync(new CreateStatementRequest
        {
            StartDate = new DateTime(2026, 3, 1),
            EndDate = new DateTime(2026, 3, 31)
        });

        ok.Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_StartAfterEnd_Fails()
    {
        var (ok, error, _) = await Sut.CreateStatementAsync(new CreateStatementRequest
        {
            ClientID = 1,
            StartDate = new DateTime(2026, 4, 1),
            EndDate = new DateTime(2026, 3, 1)
        });

        ok.Should().BeFalse();
        error.Should().Contain("start date");
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_NoActivity_FailsAndDoesNotSave()
    {
        ClientExists();
        Activity(new List<Invoice>(), new List<Payment>());

        var (ok, error, _) = await Sut.CreateStatementAsync(new CreateStatementRequest
        {
            ClientID = 1,
            StartDate = new DateTime(2026, 3, 1),
            EndDate = new DateTime(2026, 3, 31)
        });

        ok.Should().BeFalse();
        error.Should().Contain("no invoice or payment activity");
        _statements.Verify(s => s.SaveChangesAsync(), Times.Never);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_WithActivity_SavesStatement()
    {
        ClientExists();
        Activity(
            new List<Invoice>
            {
                new() { InvoiceID = 1, ClientID = 1, InvoiceDate = new DateTime(2026, 3, 5), TotalAmount = 500m, ReferenceNumber = "ANNA0001" }
            },
            new List<Payment>());

        var (ok, _, statement) = await Sut.CreateStatementAsync(new CreateStatementRequest
        {
            ClientID = 1,
            StartDate = new DateTime(2026, 3, 1),
            EndDate = new DateTime(2026, 3, 31)
        });

        ok.Should().BeTrue();
        statement!.StatementReference.Should().StartWith("STM-");
        _statements.Verify(s => s.AddAsync(statement), Times.Once);
        _statements.Verify(s => s.SaveChangesAsync(), Times.Once);
    }
    //-------------------------------------------------------------------------------
    // Balance maths

    [Fact]
    public async Task BuildData_CalculatesOpeningRunningAndClosingBalances()
    {
        ClientExists();
        Activity(
            new List<Invoice>
            {
                //Before the period: owed 1000
                new() { InvoiceID = 1, ClientID = 1, InvoiceDate = new DateTime(2026, 2, 10), TotalAmount = 1000m, ReferenceNumber = "ANNA0001" },
                //In the period: owed 500
                new() { InvoiceID = 2, ClientID = 1, InvoiceDate = new DateTime(2026, 3, 5), TotalAmount = 500m, ReferenceNumber = "ANNA0002" }
            },
            new List<Payment>
            {
                //Before the period: paid 400
                new() { InvoiceID = 1, ClientID = 1, PaymentDate = new DateTime(2026, 2, 20), Amount = 400m },
                //In the period: paid 200
                new() { InvoiceID = 2, ClientID = 1, PaymentDate = new DateTime(2026, 3, 10), Amount = 200m }
            });

        var statement = new Statement
        {
            ClientID = 1,
            StartDate = new DateTime(2026, 3, 1),
            EndDate = new DateTime(2026, 3, 31)
        };

        var (data, error) = await Sut.BuildStatementDataAsync(statement);

        error.Should().BeNull();
        data!.AccountName.Should().Be("Ann Lee");
        data.OpeningBalance.Should().Be(600m);      // 1000 - 400
        data.TotalInvoiced.Should().Be(500m);
        data.TotalPaid.Should().Be(200m);
        data.ClosingBalance.Should().Be(900m);      // 600 + 500 - 200

        data.Lines.Should().HaveCount(2);
        data.Lines[0].Type.Should().Be("Invoice");
        data.Lines[0].Balance.Should().Be(1100m);   // 600 + 500
        data.Lines[1].Type.Should().Be("Payment");
        data.Lines[1].Balance.Should().Be(900m);    // 1100 - 200
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task BuildData_EndDateIsInclusive()
    {
        ClientExists();
        DateTime? requestedEndExclusive = null;
        _invoices.Setup(i => i.GetForStatementAsync(It.IsAny<List<int>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                 .Callback<List<int>, string, DateTime>((_, _, end) => requestedEndExclusive = end)
                 .ReturnsAsync(new List<Invoice>());
        _invoices.Setup(i => i.GetPaymentsForInvoicesAsync(It.IsAny<List<int>>(), It.IsAny<DateTime>()))
                 .ReturnsAsync(new List<Payment>());

        await Sut.BuildStatementDataAsync(new Statement
        {
            ClientID = 1,
            StartDate = new DateTime(2026, 3, 1),
            EndDate = new DateTime(2026, 3, 31)
        });

        requestedEndExclusive.Should().Be(new DateTime(2026, 4, 1));
    }
    //-------------------------------------------------------------------------------
    //Lookups

    [Fact]
    public async Task BuildData_ClientNotFound_ReturnsError()
    {
        _clients.Setup(c => c.GetByIdAsync(9)).ReturnsAsync((Client?)null);

        var (data, error) = await Sut.BuildStatementDataAsync(new Statement { ClientID = 9 });

        data.Should().BeNull();
        error.Should().Be("Client not found.");
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task BuildData_OrganisationWithNoClients_ReturnsError()
    {
        _orgs.Setup(o => o.GetByIdAsync(3)).ReturnsAsync(new Organisation { OrganisationID = 3, OrganisationName = "Acme" });
        _clients.Setup(c => c.GetIdsByOrganisationAsync(3)).ReturnsAsync(new List<int>());

        var (data, error) = await Sut.BuildStatementDataAsync(new Statement { OrganisationID = 3 });

        data.Should().BeNull();
        error.Should().Be("This organisation has no clients.");
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Delete_Missing_ReturnsFalse()
    {
        _statements.Setup(s => s.GetByIdAsync(5)).ReturnsAsync((Statement?)null);

        (await Sut.DeleteStatementAsync(5)).Should().BeFalse();
    }
}
//--------------------------------------------------------------END OF FILE---------------------------------------------------------------