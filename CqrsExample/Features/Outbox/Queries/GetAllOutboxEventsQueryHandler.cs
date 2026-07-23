using CqrsExample.Contracts.Responses;
using CqrsExample.Features.Outbox.Dtos;
using CqrsExample.Repositories;
using MediatR;

namespace CqrsExample.Features.Outbox.Queries;

public record GetAllOutboxEventsQuery(int? Size, int? PageNumber)
    : IRequest<OutboxEventListResponse>;

public class GetAllOutboxEventsQueryHandler 
    : IRequestHandler<GetAllOutboxEventsQuery, OutboxEventListResponse>
{
    private readonly OutboxRepository _outboxRepository;

    public GetAllOutboxEventsQueryHandler(OutboxRepository outboxRepository)
    {
        _outboxRepository = outboxRepository;
    }

    public async Task<OutboxEventListResponse> Handle(
        GetAllOutboxEventsQuery request, CancellationToken cancellationToken)
    {
        var (size, pageNumber) = (request.Size ?? 10, request.PageNumber ?? 1);

        var (events, totalCount) = await _outboxRepository.GetEventsAsync(
            size, pageNumber, cancellationToken);

        var result = events.Select(o => new OutboxEventDto
        {
            Id = o.Id,
            EventType = o.EventType,
            Payload = o.Payload,
            IsProcessed = o.IsProcessed,
            Success = o.Success,
            ProcessAttempts = o.ProcessAttempts,
            CreatedAt = o.CreatedAt
        }).ToList();

        return new OutboxEventListResponse
        {
            Size = result.Count,
            PageNumber = pageNumber,
            TotalCount = totalCount,
            Result = result
        };
    }
}
