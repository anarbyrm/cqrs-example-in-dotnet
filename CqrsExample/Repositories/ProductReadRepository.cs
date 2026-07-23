using CqrsExample.Documents;
using CqrsExample.Features.Products.Abstractions;
using MongoDB.Driver;

namespace CqrsExample.Repositories;

public class ProductReadRepository : IProductReadRepository
{
    private readonly IMongoCollection<ProductDocument> _products;

    public ProductReadRepository(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("cqrsdb");
        _products = database.GetCollection<ProductDocument>("Products");
    }

    public async Task<IEnumerable<ProductDocument>> GetProductsAsync(
        int size, int pageNumber, CancellationToken cancellationToken)
    {
        var filter = Builders<ProductDocument>.Filter.Empty;

        return await _products
            .Find(filter)
            .Skip(pageNumber * size)
            .Limit(size)
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertProductAsync(ProductDocument product, CancellationToken cancellationToken)
    {
        var filter = Builders<ProductDocument>.Filter.Eq(p => p.Id, product.Id);

        await _products.ReplaceOneAsync(
            filter,
            product,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);
    }
}