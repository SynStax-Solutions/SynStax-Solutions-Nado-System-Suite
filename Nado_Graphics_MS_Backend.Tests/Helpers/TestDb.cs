using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Hubs;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Services;

public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _conn;

    //--------------------------------------------------------------------------
    public BMSDbContext Ctx { get; }

    //--------------------------------------------------------------------------
    public Mock<IHubContext<JobCardHub>> Hub { get; } = new();

    //--------------------------------------------------------------------------
    public Mock<IClientProxy> Proxy { get; } = new();


    //--------------------------------------------------------------------------
    public int ClientId { get; }

    //--------------------------------------------------------------------------
    public int ProductId { get; }

    //--------------------------------------------------------------------------
    public int InactiveProductId { get; }

    //--------------------------------------------------------------------------
    public TestDb()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();

        var opts = new DbContextOptionsBuilder<BMSDbContext>()
            .UseSqlite(_conn)
            .Options;

        Ctx = new BMSDbContext(opts);
        Ctx.Database.EnsureCreated();

        // Mock the SignalR hub so services can "broadcast" without a real server.
        var clients = new Mock<IHubClients>();
        clients.Setup(c => c.All).Returns(Proxy.Object);
        Hub.Setup(h => h.Clients).Returns(clients.Object);

        // Products need a category (foreign key).
        var category = new ProductCategory { CategoryName = "Signage" };
        Ctx.Add(category);
        Ctx.SaveChanges();

        
        var client = new Client { FirstName = "Jane", LastName = "Doe" };

        var product = new Product
        {
            ProductName = "Banner",
            Price = 100m,
            IsActive = true,
            ProductCategoryID = category.ProductCategoryID
        };

        var inactive = new Product
        {
            ProductName = "Old",
            Price = 50m,
            IsActive = false,
            ProductCategoryID = category.ProductCategoryID
        };

        Ctx.AddRange(client, product, inactive);
        Ctx.SaveChanges();

        ClientId = client.ClientID;
        ProductId = product.ProductID;
        InactiveProductId = inactive.ProductID;
    }

    //--------------------------------------------------------------------------
    public JobCardService JobCards() => new(Ctx, Hub.Object);

    //--------------------------------------------------------------------------
    public QuoteService Quotes() => new(Ctx, new ReferenceNumberService(Ctx), JobCards(), Hub.Object);

    //--------------------------------------------------------------------------
    public InvoiceService Invoices() => new(Ctx, new ReferenceNumberService(Ctx), JobCards());

    //--------------------------------------------------------------------------
    public Quote NewQuote(params (int qty, decimal price)[] lines) => new()
    {
        ClientID = ClientId,
        ValidUntil = DateTime.Today.AddDays(14),
        QuoteItems = lines
            .Select(l => new QuoteItem
            {
                ProductID = ProductId,
                Quantity = l.qty,
                UnitPrice = l.price
            })
            .ToList()
    };

    //--------------------------------------------------------------------------
    public void Dispose()
    {
        Ctx.Dispose();
        _conn.Dispose();
    }
}
//------------------------------------------------------END OF FILE------------------------------------------------