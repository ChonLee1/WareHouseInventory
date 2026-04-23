using WareHouseInventory.Api.Models;
using WareHouseInventory.Api.Repositories;

namespace WareHouseInventory.Api.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<Product> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
        {
            if (dto.Price < 0)
            {
                throw new ArgumentException("Price cannot be negative", nameof(dto.Price));
            }
            if (dto.StockQuantity < 0)
            {
                throw new ArgumentException("Stock quantity cannot be negative", nameof(dto.StockQuantity));
            }
            var product = new Product
            { 
                Name = dto.Name,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity
            };
            return await _repository.AddAsync(product, ct);      
        }

        public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
        {
            return _repository.GetAllAsync(ct);
        }

        public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return _repository.GetByIdAsync(id, ct);
        }

        public async Task<Product?> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct = default)
        {
            if (dto.Price < 0)
            {
                throw new ArgumentException("Price cannot be negative", nameof(dto.Price));
            }
            if (dto.StockQuantity < 0)
            {
                throw new ArgumentException("Stock quantity cannot be negative", nameof(dto.StockQuantity));
            }

            var existingProduct = await _repository.GetByIdAsync(id, ct);
            if (existingProduct is null)
            {
                return null;
            }

            existingProduct.Name = dto.Name;
            existingProduct.Price = dto.Price;
            existingProduct.StockQuantity = dto.StockQuantity;

            var updated = await _repository.UpdateAsync(existingProduct, ct);
            return updated ? existingProduct : null; 
        }
    }
}
