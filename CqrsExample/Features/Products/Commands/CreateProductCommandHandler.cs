using CqrsExample.Dtos;
using CqrsExample.Entities;
using CqrsExample.Features.Products.Abstractions;
using MediatR;

namespace CqrsExample.Features.Products.Commands;

public record CreateProductCommand(ProductCreateDto Product) : IRequest<Unit>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Unit>
{
    private readonly IProductWriteRepository _productWriteRepository;

    public CreateProductCommandHandler(IProductWriteRepository productWriteRepository)
    {
        _productWriteRepository = productWriteRepository;
    }

    public async Task<Unit> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Title = request.Product.Title,
            Description = request.Product.Description,
            Price = request.Product.Price
        };

        await _productWriteRepository.CreateProductAsync(product, cancellationToken);
        
        return Unit.Value;
    }
}