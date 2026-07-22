using CqrsExample.Repositories;

namespace CqrsExample.Workers;

public class OutboxEventCleaner : BackgroundService
{
    private const int TOTAL_EVENT_SIZE = 500;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OutboxEventCleaner> _logger;

    public OutboxEventCleaner(
        IServiceScopeFactory serviceScopeFactory, 
        ILogger<OutboxEventCleaner> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected async override Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await CleanOldOutboxEvents(ct);

                await Task.Delay(TimeSpan.FromDays(1), ct);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error cleaning old outbox events");
            }
        }
    }

    private async Task CleanOldOutboxEvents(CancellationToken ct)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        var outboxRepository = scope.ServiceProvider.GetRequiredService<OutboxRepository>();

        var deletedCount = 0;

        do
        {
            try
            {
                deletedCount = await outboxRepository.DeleteOldEventsAsync(TOTAL_EVENT_SIZE, ct);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error deleting old outbox events");
            }
        }
        while (deletedCount > 0);
    }
}