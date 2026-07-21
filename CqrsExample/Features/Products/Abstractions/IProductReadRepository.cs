using CqrsExample.Documents;

namespace CqrsExample.Features.Products.Abstractions;

public interface IProductReadRepository
{
    Task<IEnumerable<ProductDocument>> GetProductsAsync(
        int size, int pageNumber, CancellationToken cancellationToken);
}
