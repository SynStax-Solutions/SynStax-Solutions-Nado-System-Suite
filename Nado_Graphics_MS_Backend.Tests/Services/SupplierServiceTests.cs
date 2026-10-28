using FluentAssertions;
using Moq;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;
using Nado_Graphics_MS_Backend.Services;
using Xunit;

public class SupplierServiceTests
{
    private readonly Mock<ISupplierRepository> _repo = new();
    private SupplierService Sut => new(_repo.Object);

    private static Supplier NewSupplier(string name = "Acme", string? email = "a@acme.com") =>
        new() { SupplierName = name, SupplierEmail = email };
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_NewSupplier_SavesAndReturnsTrue()
    {
        var supplier = NewSupplier();
        supplier.SupplierID = 99;

        var ok = await Sut.CreateSupplierAsync(supplier);

        ok.Should().BeTrue();
        supplier.SupplierID.Should().Be(0);
        _repo.Verify(r => r.AddAsync(supplier), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_DuplicateName_ReturnsFalse_AndDoesNotSave()
    {
        _repo.Setup(r => r.NameExistsAsync("Acme", null)).ReturnsAsync(true);

        var ok = await Sut.CreateSupplierAsync(NewSupplier());

        ok.Should().BeFalse();
        _repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_DuplicateEmail_ReturnsFalse()
    {
        _repo.Setup(r => r.EmailExistsAsync("a@acme.com", null)).ReturnsAsync(true);

        (await Sut.CreateSupplierAsync(NewSupplier())).Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Update_IdMismatch_ReturnsFalse()
    {
        var supplier = NewSupplier();
        supplier.SupplierID = 5;

        (await Sut.UpdateSupplierAsync(1, supplier)).Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Update_NotFound_ReturnsFalse()
    {
        var supplier = NewSupplier();
        supplier.SupplierID = 1;
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Supplier?)null);

        (await Sut.UpdateSupplierAsync(1, supplier)).Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Update_Valid_CopiesFieldsAndSaves()
    {
        var existing = new Supplier { SupplierID = 1, SupplierName = "Old" };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        var update = NewSupplier("New");
        update.SupplierID = 1;

        var ok = await Sut.UpdateSupplierAsync(1, update);

        ok.Should().BeTrue();
        existing.SupplierName.Should().Be("New");
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Delete_Existing_RemovesAndSaves()
    {
        var existing = new Supplier { SupplierID = 1 };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

        (await Sut.DeleteSupplierAsync(1)).Should().BeTrue();
        _repo.Verify(r => r.Remove(existing), Times.Once);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Delete_Missing_ReturnsFalse()
    {
        _repo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync((Supplier?)null);

        (await Sut.DeleteSupplierAsync(7)).Should().BeFalse();
    }
}
//--------------------------------------------------------------END OF FILE---------------------------------------------------------------