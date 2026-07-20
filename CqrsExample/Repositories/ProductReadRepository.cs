using CqrsExample.Entities;
using CqrsExample.Features.Products.Abstractions;

namespace CqrsExample.Repositories;

public class ProductReadRepository : IProductReadRepository
{
    public Task<IEnumerable<Product>> GetProductsAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}