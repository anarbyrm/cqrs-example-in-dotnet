using CqrsExample.Entities;

namespace CqrsExample.Features.Products.Abstractions;

public interface IProductReadRepository
{
    Task<IEnumerable<Product>> GetProductsAsync(
        int size, int pageNumber, CancellationToken cancellationToken);
}
