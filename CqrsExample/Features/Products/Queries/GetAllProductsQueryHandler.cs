using CqrsExample.Dtos;
using CqrsExample.Features.Products.Abstractions;
using MediatR;

namespace CqrsExample.Features.Products.Queries;

public record GetAllProductsQuery() : IRequest<ProductListDto>;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, ProductListDto>
{
    private readonly IProductReadRepository _productReadRepository;

    public GetAllProductsQueryHandler(IProductReadRepository productReadRepository)
    {
        _productReadRepository = productReadRepository;
    }

    public async Task<ProductListDto> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _productReadRepository.GetProductsAsync(cancellationToken);
        // TODO: mapping to ProductListDto
        return new ProductListDto{};
    }
}