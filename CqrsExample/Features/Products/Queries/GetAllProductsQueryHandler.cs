using CqrsExample.Dtos;
using CqrsExample.Features.Products.Abstractions;
using MediatR;

namespace CqrsExample.Features.Products.Queries;

public record GetAllProductsQuery() : IRequest<IEnumerable<ProductListDto>>;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductListDto>>
{
    private readonly IProductReadRepository _productReadRepository;

    public GetAllProductsQueryHandler(IProductReadRepository productReadRepository)
    {
        _productReadRepository = productReadRepository;
    }

    public async Task<IEnumerable<ProductListDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _productReadRepository.GetProductsAsync(cancellationToken);
        
        return products.Select(p => new ProductListDto
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            Price = p.Price
        }).ToList();
    }
}