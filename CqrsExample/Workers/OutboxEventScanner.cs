using CqrsExample.Repositories;
using CqrsExample.Services;

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
        _logger.LogInformation("{Worker} started", nameof(OutboxEventScanner));

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxEvents(ct);

                await Task.Delay(TimeSpan.FromSeconds(10), ct);

            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "{Worker} failed while processing outbox events", nameof(OutboxEventScanner));
            }
        }

        _logger.LogInformation("{Worker} stopped", nameof(OutboxEventScanner));
    }

    private async Task ProcessOutboxEvents(CancellationToken ct)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        var outboxRepository = scope.ServiceProvider.GetRequiredService<OutboxRepository>();
        var brokerService = scope.ServiceProvider.GetRequiredService<RabbitmqService>();

        var outboxEvents = await outboxRepository.FetchUnprocessedEventsAsync(TOTAL_EVENT_SIZE, ct);

        if (outboxEvents.Count == 0)
        {
            return;
        }

        _logger.LogInformation("{Worker} fetched {Count} unprocessed outbox event(s)", nameof(OutboxEventScanner), outboxEvents.Count);

        foreach (var outboxEvent in outboxEvents)
        {
            try
            {
                await brokerService.PublishEvent(
                    outboxEvent.EventType, outboxEvent.Payload, ct);

                await outboxRepository.RecordProcessAttemptAsync(outboxEvent.Id, success: true, ct);

                _logger.LogInformation("{Worker} published outbox event {OutboxEventId} of type {EventType}", nameof(OutboxEventScanner), outboxEvent.Id, outboxEvent.EventType);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "{Worker} failed to process outbox event with id {OutboxEventId}", nameof(OutboxEventScanner), outboxEvent.Id);

                await outboxRepository.RecordProcessAttemptAsync(outboxEvent.Id, success: false, ct);
            }
        }
    }
}