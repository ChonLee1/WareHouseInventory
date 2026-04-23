using WareHouseInventory.Api.Models;

namespace WareHouseInventory.Api.Services
{
    public interface IProductService
    {
        Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
        Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Product> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
        Task<Product?> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct = default);
    }
}
