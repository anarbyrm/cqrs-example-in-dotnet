using CqrsExample.Contracts.Responses;
using CqrsExample.Dtos;
using CqrsExample.Features.Products.Abstractions;
using MediatR;

namespace CqrsExample.Features.Products.Queries;

public record GetAllProductsQuery(int? Size, int? PageNumber) 
    : IRequest<ProductListResponse>;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, ProductListResponse>
{
    private readonly IProductReadRepository _productReadRepository;

    public GetAllProductsQueryHandler(IProductReadRepository productReadRepository)
    {
        _productReadRepository = productReadRepository;
    }

    public async Task<ProductListResponse> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var (size, pageNumber) = (request.Size ?? 10, request.PageNumber ?? 1);

        var products = await _productReadRepository.GetProductsAsync(
            size, pageNumber, cancellationToken);
        
        var productResult = products.Select(p => new ProductListDto
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            Price = p.Price
        }).ToList();

        return new ProductListResponse
        {
            Size = productResult.Count,
            PageNumber = pageNumber,
            Result = productResult
        };
    }
}