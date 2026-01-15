using InMemoryWebApiPractice.DTOs;
using InMemoryWebApiPractice.Models;

namespace InMemoryWebApiPractice.Services;

public interface IProductService
{
    Task<Product> Get(Guid id);
    Task<List<Product>> GetAll();
    Task Add(ProductCreationDto product);
}