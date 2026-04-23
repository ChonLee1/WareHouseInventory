using WareHouseInventory.Api.Models;

namespace WareHouseInventory.Api.Repositories
{
    public interface IProductRepository
    {
        Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken = default);
    }
}
