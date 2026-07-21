using System.Text.Json;
using CqrsExample.Contexts;
using CqrsExample.Dtos;
using CqrsExample.Entities;
using CqrsExample.Features.Products.Abstractions;
using CqrsExample.Repositories;
using MediatR;

namespace CqrsExample.Features.Products.Commands;

public record CreateProductCommand(ProductCreateDto Product) : IRequest<Unit>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Unit>
{
    private readonly CommandDbContext _dbContext;
    private readonly IProductWriteRepository _productWriteRepository;
    private readonly OutboxRepository _outboxRepository;

    public CreateProductCommandHandler(
        CommandDbContext dbContext,
        IProductWriteRepository productWriteRepository,
        OutboxRepository outboxRepository)
    {
        _dbContext = dbContext;
        _productWriteRepository = productWriteRepository;
        _outboxRepository = outboxRepository;
    }

    public async Task<Unit> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {

        await using var transaction = 
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var product = new Product
            {
                Title = request.Product.Title,
                Description = request.Product.Description,
                Price = request.Product.Price
            };

            await _productWriteRepository.CreateProductAsync(product, cancellationToken);

            await _outboxRepository.AddOutboxEventAsync(new Outbox
            {
                EventType = "ProductCreated",
                Payload = JsonSerializer.Serialize(product),
                IsProcessed = false,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Unit.Value;

        }
        catch (Exception exc)
        {
            // TODO: logging
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}