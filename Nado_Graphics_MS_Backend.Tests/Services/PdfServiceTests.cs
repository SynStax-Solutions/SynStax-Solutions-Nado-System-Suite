using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;
using Nado_Graphics_MS_Backend.Services;
using Xunit;

public class PdfServiceTests
{
    private readonly Mock<IRepository<Client>> _clients = new();
    private readonly Mock<IProductRepository> _products = new();
    private readonly IOptions<CompanyInfoOptions> _company = Options.Create(new CompanyInfoOptions());
    private readonly Mock<IWebHostEnvironment> _env = new();
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task InvoicePdf_InvoiceNotFound_ReturnsNull()
    {
        var invoices = new Mock<IInvoiceRepository>();
        invoices.Setup(r => r.GetWithItemsAsync(1)).ReturnsAsync((Invoice?)null);
        var sut = new InvoicePDFService(invoices.Object, _clients.Object, _products.Object, _company, _env.Object);

        (await sut.GenerateInvoicePdfAsync(1, "Sam")).Should().BeNull();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task InvoicePdf_ClientNotFound_ReturnsNull()
    {
        var invoices = new Mock<IInvoiceRepository>();
        invoices.Setup(r => r.GetWithItemsAsync(1)).ReturnsAsync(new Invoice { InvoiceID = 1, ClientID = 7 });
        _clients.Setup(c => c.GetByIdAsync(7)).ReturnsAsync((Client?)null);
        var sut = new InvoicePDFService(invoices.Object, _clients.Object, _products.Object, _company, _env.Object);

        (await sut.GenerateInvoicePdfAsync(1, "Sam")).Should().BeNull();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task QuotePdf_QuoteNotFound_ReturnsNull()
    {
        var quotes = new Mock<IQuoteRepository>();
        quotes.Setup(r => r.GetWithItemsAsync(1)).ReturnsAsync((Quote?)null);
        var sut = new QuotePDFService(quotes.Object, _clients.Object, _products.Object, _company, _env.Object);

        (await sut.GenerateQuotePdfAsync(1, "Sam")).Should().BeNull();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task QuotePdf_ClientNotFound_ReturnsNull()
    {
        var quotes = new Mock<IQuoteRepository>();
        quotes.Setup(r => r.GetWithItemsAsync(1)).ReturnsAsync(new Quote { QuoteID = 1, ClientID = 7 });
        _clients.Setup(c => c.GetByIdAsync(7)).ReturnsAsync((Client?)null);
        var sut = new QuotePDFService(quotes.Object, _clients.Object, _products.Object, _company, _env.Object);

        (await sut.GenerateQuotePdfAsync(1, "Sam")).Should().BeNull();
    }
}
//---------------------------------------------------------END OF FILE-------------------------------------------------------------------