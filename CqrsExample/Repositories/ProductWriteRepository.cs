using CqrsExample.Contexts;
using CqrsExample.Entities;
using CqrsExample.Features.Products.Abstractions;

namespace CqrsExample.Repositories;

public class ProductWriteRepository : IProductWriteRepository
{
    private readonly CommandDbContext _dbContext;

    public ProductWriteRepository(CommandDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateProductAsync(Product product, CancellationToken cancellationToken)
    {
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}