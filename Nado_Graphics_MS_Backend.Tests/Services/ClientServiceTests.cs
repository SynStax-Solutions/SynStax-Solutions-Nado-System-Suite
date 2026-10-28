using FluentAssertions;
using Moq;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;
using Nado_Graphics_MS_Backend.Services;
using Xunit;

public class ClientServiceTests
{
    private readonly Mock<IRepository<Client>> _clients = new();
    private readonly Mock<IOrganisationRepository> _orgs = new();
    private ClientService Sut => new(_clients.Object, _orgs.Object);
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_WithUnknownOrganisation_ReturnsFalse()
    {
        _orgs.Setup(o => o.GetByIdAsync(9)).ReturnsAsync((Organisation?)null);

        var ok = await Sut.CreateClientAsync(new Client { OrganisationID = 9 });

        ok.Should().BeFalse();
        _clients.Verify(c => c.SaveChangesAsync(), Times.Never);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_WithOrganisation_CopiesOrganisationName()
    {
        _orgs.Setup(o => o.GetByIdAsync(3))
             .ReturnsAsync(new Organisation { OrganisationID = 3, OrganisationName = "Acme" });
        var client = new Client { OrganisationID = 3, ClientID = 77 };

        var ok = await Sut.CreateClientAsync(client);

        ok.Should().BeTrue();
        client.ClientID.Should().Be(0);
        client.OrganisationName.Should().Be("Acme");
        client.PasswordHash.Should().Be("N/A");
        _clients.Verify(c => c.SaveChangesAsync(), Times.Once);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_WithoutOrganisation_ClearsOrganisationName()
    {
        var client = new Client { OrganisationName = "Stale" };

        (await Sut.CreateClientAsync(client)).Should().BeTrue();
        client.OrganisationName.Should().BeNull();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Update_BlankPassword_KeepsExistingHash()
    {
        var existing = new Client { ClientID = 1, PasswordHash = "oldhash" };
        _clients.Setup(c => c.GetByIdAsync(1)).ReturnsAsync(existing);

        await Sut.UpdateClientAsync(1, new Client { ClientID = 1, PasswordHash = "" });

        existing.PasswordHash.Should().Be("oldhash");
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Delete_Missing_ReturnsFalse()
    {
        _clients.Setup(c => c.GetByIdAsync(5)).ReturnsAsync((Client?)null);

        (await Sut.DeleteClientAsync(5)).Should().BeFalse();
    }
}
//--------------------------------------------------------------END OF FILE---------------------------------------------------------------