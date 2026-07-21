using CqrsExample.Contexts;
using CqrsExample.Entities;

namespace CqrsExample.Repositories;

public class OutboxRepository
{
    private readonly CommandDbContext _dbContext;

    public OutboxRepository(CommandDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddOutboxEventAsync(Outbox outboxEvent, CancellationToken cancellationToken)
    {
        _dbContext.Outbox.Add(outboxEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}