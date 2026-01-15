using InMemoryWebApiPractice.Models;
using Microsoft.EntityFrameworkCore;

namespace InMemoryWebApiPractice.Persistence;

public class AppDbContext : DbContext
{

    public AppDbContext(DbContextOptions<AppDbContext> contextOptions) : base(contextOptions)
    {
    }
    public DbSet<Product> Products { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasKey(p => p.Id);
    }
}