using CqrsExample.Entities;
using CqrsExample.Features.Products.Abstractions;

namespace CqrsExample.Repositories;

public class ProductWriteRepository : IProductWriteRepository
{
    public Task CreateProductAsync(Product product, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}