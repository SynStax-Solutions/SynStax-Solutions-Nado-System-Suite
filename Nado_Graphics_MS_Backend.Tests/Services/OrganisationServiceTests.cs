using FluentAssertions;
using Moq;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;
using Nado_Graphics_MS_Backend.Services;
using Xunit;

public class OrganisationServiceTests
{
    private readonly Mock<IOrganisationRepository> _repo = new();
    private OrganisationService Sut => new(_repo.Object);

    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_DuplicateName_ReturnsDuplicate()
    {
        _repo.Setup(r => r.NameExistsAsync("Acme", null)).ReturnsAsync(true);

        var outcome = await Sut.CreateOrganisationAsync(new Organisation { OrganisationName = "Acme" });

        outcome.Should().Be(OrganisationOutcome.DuplicateName);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_New_ResetsIdAndSaves()
    {
        var org = new Organisation { OrganisationName = "Acme", OrganisationID = 50 };

        var outcome = await Sut.CreateOrganisationAsync(org);

        outcome.Should().Be(OrganisationOutcome.Ok);
        org.OrganisationID.Should().Be(0);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Update_IdMismatch_ReturnsIdMismatch()
    {
        var outcome = await Sut.UpdateOrganisationAsync(1, new Organisation { OrganisationID = 2 });
        outcome.Should().Be(OrganisationOutcome.IdMismatch);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Update_Missing_ReturnsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Organisation?)null);

        var outcome = await Sut.UpdateOrganisationAsync(1, new Organisation { OrganisationID = 1 });
        outcome.Should().Be(OrganisationOutcome.NotFound);
    }
}
//--------------------------------------------------------------END OF FILE---------------------------------------------------------------