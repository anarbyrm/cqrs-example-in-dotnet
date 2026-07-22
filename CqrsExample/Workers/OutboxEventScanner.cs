using CqrsExample.Repositories;

namespace CqrsExample.Workers;

public class OutboxEventScanner : BackgroundService
{
    private const int TOTAL_EVENT_SIZE = 100;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OutboxEventScanner> _logger;

    public OutboxEventScanner(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OutboxEventScanner> logger)
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
                await ProcessOutboxEvents(ct);

                await Task.Delay(TimeSpan.FromSeconds(10), ct);

            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error processing outbox events");
            }
        }

    }

    private async Task ProcessOutboxEvents(CancellationToken ct)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        var outboxRepository = scope.ServiceProvider.GetRequiredService<OutboxRepository>();

        var outboxEvents = await outboxRepository.FetchUnprocessedEventsAsync(TOTAL_EVENT_SIZE, ct);

        foreach (var outboxEvent in outboxEvents)
        {
            try
            {
                // TODO: publish to broker
                await outboxRepository.MarkEventAsProcessedAsync(outboxEvent.Id, ct);

            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error processing outbox event with id {OutboxEventId}", outboxEvent.Id);
            }
        }
    }
}