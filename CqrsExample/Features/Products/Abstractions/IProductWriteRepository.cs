using CqrsExample.Entities;

namespace CqrsExample.Features.Products.Abstractions;

public interface IProductWriteRepository
{
    Task CreateProductAsync(Product product, CancellationToken cancellationToken);
}
