using CqrsExample.Entities;
using CqrsExample.Features.Products.Abstractions;
using MongoDB.Driver;

namespace CqrsExample.Repositories;

public class ProductReadRepository : IProductReadRepository
{
    private readonly IMongoCollection<Product> _products;

    public ProductReadRepository(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("cqrsdb");
        _products = database.GetCollection<Product>("Products");
    }

    public async Task<IEnumerable<Product>> GetProductsAsync(
        int size, int pageNumber, CancellationToken cancellationToken)
    {
        var filter = Builders<Product>.Filter.Empty;

        return await _products
            .Find(filter)
            .Skip(pageNumber * size)
            .Limit(size)
            .ToListAsync(cancellationToken);
    }
}