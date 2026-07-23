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
        _logger.LogInformation("{Worker} started", nameof(OutboxEventCleaner));

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await CleanOldOutboxEvents(ct);

                await Task.Delay(TimeSpan.FromDays(1), ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "{Worker} failed while cleaning old outbox events", nameof(OutboxEventCleaner));
            }
        }

        _logger.LogInformation("{Worker} stopped", nameof(OutboxEventCleaner));
    }

    private async Task CleanOldOutboxEvents(CancellationToken ct)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        var outboxRepository = scope.ServiceProvider.GetRequiredService<OutboxRepository>();

        var deletedCount = 0;
        var totalDeleted = 0;

        do
        {
            try
            {
                deletedCount = await outboxRepository.DeleteOldEventsAsync(TOTAL_EVENT_SIZE, ct);
                totalDeleted += deletedCount;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "{Worker} failed to delete old outbox events", nameof(OutboxEventCleaner));
                break;
            }
        }
        while (deletedCount > 0);

        if (totalDeleted > 0)
        {
            _logger.LogInformation("{Worker} deleted {Count} old outbox event(s)", nameof(OutboxEventCleaner), totalDeleted);
        }
    }
}