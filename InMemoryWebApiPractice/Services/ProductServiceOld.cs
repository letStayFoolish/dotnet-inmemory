using InMemoryWebApiPractice.DTOs;
using InMemoryWebApiPractice.Models;
using InMemoryWebApiPractice.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace InMemoryWebApiPractice.Services;

public class ProductServiceOld(AppDbContext context, IMemoryCache cache, ILogger<ProductServiceOld> logger) : IProductService
{
    public async Task Add(ProductCreationDto request)
    {
        var product = new Product(request.Name, request.Description, request.Price);
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();
        // invalidate cache for products, as new product is added
        var cacheKey = "products";
        logger.LogInformation("invalidating cache for key: {CacheKey} from cache.", cacheKey);
        cache.Remove(cacheKey);//Manual Invalidation ->  ensuring that the list of products is removed from the cache.
    }

    public async Task<Product> Get(Guid id)
    {
        var cacheKey = $"product:{id}";
        logger.LogInformation("fetching data for key: {CacheKey} from cache.", cacheKey);
        if (!cache.TryGetValue(cacheKey, out Product? product))
        {
            logger.LogInformation("cache miss. fetching data for key: {CacheKey} from database.", cacheKey);
            product = await context.Products.FindAsync(id);
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(50))
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetPriority(CacheItemPriority.Normal);
            logger.LogInformation("setting data for key: {CacheKey} to cache.", cacheKey);
            cache.Set(cacheKey, product, cacheOptions);
        }
        else
        {
            logger.LogInformation("cache hit for key: {CacheKey}.", cacheKey);
        }

        return product;
    }

    public async Task<List<Product>> GetAll()
    {
        var cacheKey = "products";
        logger.LogInformation("fetching data for key: {CacheKey} from cache.", cacheKey);
        if (!cache.TryGetValue(cacheKey, out List<Product>? products))
        {
            logger.LogInformation("cache miss. fetching data for key: {CacheKey} from database.", cacheKey);
            products = await context.Products.ToListAsync();
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))// Defines a fixed time after which the cache entry will expire, regardless of how often it is accessed. 
                .SetSlidingExpiration(TimeSpan.FromMinutes(2))//a 2-minute sliding expiration means that if no one accesses the cache entry within 2 minutes, it will be removed.
                .SetPriority(CacheItemPriority.NeverRemove)//Sets the priority of retaining the cache entry. This determines the likelihood of the entry being removed when the cache exceeds memory limits.(Normal - default)
                .SetSize(2048);//Specifies the size of the cache entry. This helps prevent the cache from consuming excessive server resources.
            logger.LogInformation("setting data for key: {CacheKey} to cache.", cacheKey);
            cache.Set(cacheKey, products, cacheOptions);
        }
        else
        {
            logger.LogInformation("cache hit for key: {CacheKey}.", cacheKey);
        }

        return products;
    }
}