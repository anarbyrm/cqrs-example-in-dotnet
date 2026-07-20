using CqrsExample.Dtos;
using MediatR;

namespace CqrsExample.Features.Products.Queries;

public record GetAllProductsQuery() : IRequest<ProductListDto>;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, ProductListDto>
{
    public async Task<ProductListDto> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        // TODO: further logic here ...
        return new ProductListDto
        {
            Id = 1,
            Title = "Sample Product",
            Description = "This is a sample product.",
            Price = 9.99m
        };
    }
}