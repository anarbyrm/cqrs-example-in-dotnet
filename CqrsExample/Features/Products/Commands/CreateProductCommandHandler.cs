using MediatR;

namespace CqrsExample.Features.Products.Commands;

public record CreateProductCommand() : IRequest<Unit>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Unit>
{
    public async Task<Unit> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // TODO: further logic here ...
        return Unit.Value;
    }
}