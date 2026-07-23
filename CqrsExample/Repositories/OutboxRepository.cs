using CqrsExample.Contexts;
using CqrsExample.Entities;
using Microsoft.EntityFrameworkCore;

namespace CqrsExample.Repositories;

public class OutboxRepository
{
    private readonly CommandDbContext _dbContext;

    public OutboxRepository(CommandDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddOutboxEventAsync(Outbox outboxEvent, CancellationToken ct)
    {
        await _dbContext.Outbox.AddAsync(outboxEvent, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<ICollection<Outbox>> FetchUnprocessedEventsAsync(int count, CancellationToken ct)
    {
        return await _dbContext.Outbox
            .Where(o => !o.IsProcessed)
            .Where(o => o.ProcessAttempts < 3)
            .OrderBy(o => o.Id)
            .Take(count)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Fetches outbox events which have been processed and are 7 days or more older. 
    /// </summary>
    /// <param name="count"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<int> DeleteOldEventsAsync(int count, CancellationToken ct)
    {
        var toBeDeletedAt = DateTime.UtcNow.AddDays(-7);

        return await _dbContext.Outbox
            .Where(o => o.IsProcessed)
            .Where(o => o.CreatedAt <= toBeDeletedAt)
            .OrderBy(o => o.Id)
            .Take(count)
            .ExecuteDeleteAsync(ct);
    }

    public async Task RecordProcessAttemptAsync(int outboxId, bool success, CancellationToken ct)
    {
        await _dbContext.Outbox
            .Where(o => o.Id == outboxId)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(o => o.ProcessAttempts, o => o.ProcessAttempts + 1)
                .SetProperty(o => o.Success, success)
                .SetProperty(o => o.IsProcessed, success), ct);
    }
}