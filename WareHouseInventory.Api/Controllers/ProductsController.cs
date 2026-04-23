using Microsoft.AspNetCore.Mvc;
using WareHouseInventory.Api.Models;
using WareHouseInventory.Api.Services;

namespace WareHouseInventory.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductsController(IProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Product>>> GetAll(CancellationToken ct)
        {
            var products = await _service.GetAllAsync(ct);
            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetById(int id, CancellationToken ct)
        {
            var product = await _service.GetByIdAsync(id, ct);
            if (product is null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> Create([FromBody] CreateProductDto dto, CancellationToken ct)
        {
            var createdProduct = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Product>> Update(int id, [FromBody] UpdateProductDto dto, CancellationToken ct)
        { 
            var updatedProduct = await _service.UpdateAsync(id, dto, ct);
            if (updatedProduct is null)
            {
                return NotFound();
            } 
            return Ok(updatedProduct);
        }

    }
}
