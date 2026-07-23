using CqrsExample.Features.Outbox.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CqrsExample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OutboxController : ControllerBase
{
    private readonly IMediator _mediator;

    public OutboxController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? size,
        [FromQuery] int? pageNumber,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllOutboxEventsQuery(size, pageNumber), cancellationToken);
        return Ok(result);
    }
}
