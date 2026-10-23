using FluentAssertions;
using Moq;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;
using Nado_Graphics_MS_Backend.Services;
using Xunit;

public class ProductServiceTests
{
    private readonly Mock<IRepository<Product>> _products = new();
    private readonly Mock<IProductCategoryRepository> _categories = new();
    private ProductService Sut => new(_products.Object, _categories.Object);
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_UnknownCategory_ReturnsFalse()
    {
        _categories.Setup(c => c.GetByIdAsync(1)).ReturnsAsync((ProductCategory?)null);

        (await Sut.CreateProductAsync(new Product { ProductCategoryID = 1, Price = 10 })).Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_NegativePrice_ReturnsFalse()
    {
        _categories.Setup(c => c.GetByIdAsync(1)).ReturnsAsync(new ProductCategory());

        (await Sut.CreateProductAsync(new Product { ProductCategoryID = 1, Price = -5 })).Should().BeFalse();
        _products.Verify(p => p.SaveChangesAsync(), Times.Never);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_Valid_SavesProduct()
    {
        _categories.Setup(c => c.GetByIdAsync(1)).ReturnsAsync(new ProductCategory());
        var product = new Product { ProductCategoryID = 1, Price = 99 };

        (await Sut.CreateProductAsync(product)).Should().BeTrue();
        _products.Verify(p => p.AddAsync(product), Times.Once);
        _products.Verify(p => p.SaveChangesAsync(), Times.Once);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Update_IdMismatch_ReturnsFalse()
    {
        (await Sut.UpdateProductAsync(1, new Product { ProductID = 2 })).Should().BeFalse();
    }
}
//--------------------------------------------------------------END OF FILE---------------------------------------------------------------