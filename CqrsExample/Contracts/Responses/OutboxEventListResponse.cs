using CqrsExample.Features.Outbox.Dtos;

namespace CqrsExample.Contracts.Responses;

public class OutboxEventListResponse
{
    public int Size { get; set; }
    public int PageNumber { get; set; }
    public int TotalCount { get; set; }
    public IEnumerable<OutboxEventDto> Result { get; set; } = null!;
}
