using InMemoryWebApiPractice.DTOs;
using InMemoryWebApiPractice.Persistence;
using InMemoryWebApiPractice.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("dotnetSeries"));
});
builder.Services.AddMemoryCache();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    // It sets the Redis server’s connection string to “localhost”, indicating that the Redis server is running on the local machine.
    options.Configuration = "localhost";
    options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions()
    {
        AbortOnConnectFail = true,//meaning the application will fail immediately if it cannot connect to Redis.
        EndPoints = { options.Configuration }//property is configured to use the same connection string specified in options.Configuration, ensuring that the endpoint for the Redis server is correctly set.
    };
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.DisplayRequestDuration());
}

app.MapGet("/products", async (IProductService service) =>
{
    var products = await service.GetAll();
    return Results.Ok(products);
});

app.MapGet("/products/{id:guid}", async (Guid id, IProductService service) =>
{
    var product = await service.Get(id);
    return Results.Ok(product);
});

app.MapPost("/products", async (ProductCreationDto product, IProductService service) =>
{
    await service.Add(product);
    return Results.Created();
});

app.UseHttpsRedirection();
app.Run();