using CqrsExample.Entities;

namespace CqrsExample.Features.Products.Abstractions;

public interface IProductReadRepository
{
    Task<IEnumerable<Product>> GetProductsAsync(CancellationToken cancellationToken);
}
