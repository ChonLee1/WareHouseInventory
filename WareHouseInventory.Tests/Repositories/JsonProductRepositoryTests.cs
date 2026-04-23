using WareHouseInventory.Api.Models;
using WareHouseInventory.Api.Repositories;

namespace WareHouseInventory.Tests.Repositories;

public class JsonProductRepositoryTests : IDisposable
{
    private readonly string _tmpFile;

    public JsonProductRepositoryTests()
    {
        // Unikátny tmp súbor pre každý test, aby sa testy navzájom nerušili.
        _tmpFile = Path.Combine(Path.GetTempPath(), $"inv_{Guid.NewGuid():N}.json");
    }

    public void Dispose()
    {
        if (File.Exists(_tmpFile))
            File.Delete(_tmpFile);
    }

    // ---------- GetAllAsync ----------

    [Fact]
    public async Task GetAllAsync_FileDoesNotExist_ReturnsEmpty()
    {
        // Arrange — tmp súbor sme nezapísali, takže neexistuje.
        var repo = new JsonProductRepository(_tmpFile);

        // Act
        var result = await repo.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    // ---------- AddAsync ----------

    [Fact]
    public async Task AddAsync_FirstProduct_AssignsIdOne()
    {
        // Arrange
        var repo = new JsonProductRepository(_tmpFile);
        var product = new Product { Name = "Kladivo", Price = 9.99m, StockQuantity = 5 };

        // Act
        var added = await repo.AddAsync(product);

        // Assert
        Assert.Equal(1, added.Id);
        Assert.Equal("Kladivo", added.Name);
    }

    [Fact]
    public async Task AddAsync_MultipleProducts_AssignsIncrementingIds()
    {
        // Arrange
        var repo = new JsonProductRepository(_tmpFile);

        // Act
        var a = await repo.AddAsync(new Product { Name = "A", Price = 1m, StockQuantity = 1 });
        var b = await repo.AddAsync(new Product { Name = "B", Price = 2m, StockQuantity = 2 });
        var c = await repo.AddAsync(new Product { Name = "C", Price = 3m, StockQuantity = 3 });

        // Assert
        Assert.Equal(1, a.Id);
        Assert.Equal(2, b.Id);
        Assert.Equal(3, c.Id);
    }

    [Fact]
    public async Task AddAsync_PersistsToFile()
    {
        // Arrange — pridáme produkt jednou inštanciou repa.
        var repoA = new JsonProductRepository(_tmpFile);
        await repoA.AddAsync(new Product { Name = "Persistent", Price = 5m, StockQuantity = 2 });

        // Act — načítame novou inštanciou (simuluje reštart appky).
        var repoB = new JsonProductRepository(_tmpFile);
        var result = await repoB.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Persistent", result[0].Name);
    }

    // ---------- GetByIdAsync ----------

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsProduct()
    {
        // Arrange
        var repo = new JsonProductRepository(_tmpFile);
        var added = await repo.AddAsync(new Product { Name = "X", Price = 1m, StockQuantity = 1 });

        // Act
        var found = await repo.GetByIdAsync(added.Id);

        // Assert
        Assert.NotNull(found);
        Assert.Equal(added.Id, found.Id);
        Assert.Equal("X", found.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Arrange — prázdne repo.
        var repo = new JsonProductRepository(_tmpFile);

        // Act
        var result = await repo.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    // ---------- UpdateAsync ----------

    [Fact]
    public async Task UpdateAsync_ExistingId_UpdatesAndReturnsTrue()
    {
        // Arrange
        var repo = new JsonProductRepository(_tmpFile);
        var added = await repo.AddAsync(new Product { Name = "Old", Price = 1m, StockQuantity = 1 });

        var modified = new Product
        {
            Id = added.Id,
            Name = "New",
            Price = 9.99m,
            StockQuantity = 42
        };

        // Act
        var success = await repo.UpdateAsync(modified);

        // Assert
        Assert.True(success);

        // Overíme, že zmena sa aj reálne zapísala (nové načítanie zo súboru).
        var reloaded = await repo.GetByIdAsync(added.Id);
        Assert.NotNull(reloaded);
        Assert.Equal("New", reloaded.Name);
        Assert.Equal(9.99m, reloaded.Price);
        Assert.Equal(42, reloaded.StockQuantity);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentId_ReturnsFalse()
    {
        // Arrange
        var repo = new JsonProductRepository(_tmpFile);
        var phantom = new Product { Id = 999, Name = "Ghost", Price = 1m, StockQuantity = 1 };

        // Act
        var success = await repo.UpdateAsync(phantom);

        // Assert
        Assert.False(success);
    }
}
