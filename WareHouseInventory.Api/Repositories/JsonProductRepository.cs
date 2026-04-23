using System.Text.Json;
using WareHouseInventory.Api.Models;

namespace WareHouseInventory.Api.Repositories
{
    public class JsonProductRepository : IProductRepository
    {
        private readonly string _filePath;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public JsonProductRepository(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<Product> AddAsync(Product product, CancellationToken ct = default)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                var products = await LoadAsync(ct);
                product.Id = products.Count > 0 ? products.Max(p => p.Id) + 1 : 1;
                products.Add(product);
                await SaveAsync(products, ct);
                return product;
            }
            finally { 
                _semaphore.Release(); 
            }
        }

        public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct= default)
        {
            await _semaphore.WaitAsync(ct);
            try {
                return await LoadAsync(ct);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            await _semaphore.WaitAsync(ct);
            try {
                var products = await LoadAsync(ct);
                return products.FirstOrDefault(p => p.Id == id);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> UpdateAsync(Product product, CancellationToken ct = default)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
               var products = await LoadAsync(ct);
                var index = products.FindIndex(p => p.Id == product.Id);
                if (index >= 0)
                {
                    products[index] = product;
                    await SaveAsync(products, ct);
                    return true;
                    
                }
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<List<Product>> LoadAsync(CancellationToken ct) { 
            if (!File.Exists(_filePath))
            {
                return new List<Product>();
            }
            await using var stream = File.OpenRead(_filePath);
            if (stream.Length == 0)
            {
                return new List<Product>();
            }

            var products = await JsonSerializer.DeserializeAsync<List<Product>>(stream, _jsonOptions, ct);
            return products ?? new List<Product>();
        }

        private async Task SaveAsync(List<Product> products, CancellationToken ct)
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) 
            { 
                Directory.CreateDirectory(dir);
            }
            await using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, products, _jsonOptions, ct);
        }
    }
}
