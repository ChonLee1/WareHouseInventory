using WareHouseInventory.Api.Repositories;
using WareHouseInventory.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IProductRepository>(_ =>
{
    var filePath = Path.Combine(builder.Environment.ContentRootPath, "inventory.json");
    return new JsonProductRepository(filePath);
});

builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
