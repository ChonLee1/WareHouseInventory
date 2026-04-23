using Moq;
using WareHouseInventory.Api.Models;
using WareHouseInventory.Api.Repositories;
using WareHouseInventory.Api.Services;

namespace WareHouseInventory.Tests.Services;

public class ProductServiceTests
{
    // ---------- GetByIdAsync ----------

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsProduct()
    {
        // Arrange
        var expected = new Product { Id = 5, Name = "Kladivo", Price = 9.99m, StockQuantity = 10 };
        var repoMock = new Mock<IProductRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var service = new ProductService(repoMock.Object);

        // Act
        var result = await service.GetByIdAsync(5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("Kladivo", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        var repoMock = new Mock<IProductRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(repoMock.Object);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    // ---------- GetAllAsync ----------

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "A", Price = 1m, StockQuantity = 1 },
            new() { Id = 2, Name = "B", Price = 2m, StockQuantity = 2 }
        };
        var repoMock = new Mock<IProductRepository>();
        repoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var service = new ProductService(repoMock.Object);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "A");
        Assert.Contains(result, p => p.Name == "B");
    }

    // ---------- CreateAsync ----------

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsCreatedProductWithId()
    {
        // Arrange
        var dto = new CreateProductDto { Name = "Skrutka", Price = 0.5m, StockQuantity = 100 };

        var repoMock = new Mock<IProductRepository>();
        repoMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) =>
            {
                p.Id = 1;           // fake "db pridelilo Id"
                return p;
            });

        var service = new ProductService(repoMock.Object);

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Skrutka", result.Name);
        Assert.Equal(0.5m, result.Price);
        Assert.Equal(100, result.StockQuantity);

        repoMock.Verify(
            r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NegativePrice_ThrowsArgumentException()
    {
        // Arrange
        var repoMock = new Mock<IProductRepository>();
        var service = new ProductService(repoMock.Object);
        var dto = new CreateProductDto { Name = "Bad", Price = -1m, StockQuantity = 5 };

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));

        // Repo nesmel byť dotknutý — validácia service prerušila tok skôr
        repoMock.Verify(
            r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_NegativeStockQuantity_ThrowsArgumentException()
    {
        // Arrange
        var repoMock = new Mock<IProductRepository>();
        var service = new ProductService(repoMock.Object);
        var dto = new CreateProductDto { Name = "Bad", Price = 1m, StockQuantity = -5 };

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));

        repoMock.Verify(
            r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ---------- UpdateAsync ----------

    [Fact]
    public async Task UpdateAsync_ExistingId_ReturnsUpdatedProduct()
    {
        // Arrange
        var existing = new Product { Id = 1, Name = "Old", Price = 1m, StockQuantity = 1 };
        var dto = new UpdateProductDto { Name = "New", Price = 2m, StockQuantity = 20 };

        var repoMock = new Mock<IProductRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new ProductService(repoMock.Object);

        // Act
        var result = await service.UpdateAsync(1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("New", result.Name);
        Assert.Equal(2m, result.Price);
        Assert.Equal(20, result.StockQuantity);

        repoMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        var dto = new UpdateProductDto { Name = "X", Price = 1m, StockQuantity = 1 };

        var repoMock = new Mock<IProductRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(repoMock.Object);

        // Act
        var result = await service.UpdateAsync(999, dto);

        // Assert
        Assert.Null(result);

        // Keď sa nenašiel, repo.UpdateAsync sa nesmel volať
        repoMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_NegativePrice_ThrowsArgumentException()
    {
        // Arrange
        var repoMock = new Mock<IProductRepository>();
        var service = new ProductService(repoMock.Object);
        var dto = new UpdateProductDto { Name = "Bad", Price = -1m, StockQuantity = 5 };

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateAsync(1, dto));

        // Ani GetById, ani UpdateAsync na repe — validácia prerušila skôr
        repoMock.Verify(
            r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        repoMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
