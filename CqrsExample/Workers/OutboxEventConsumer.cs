namespace CqrsExample.Workers;

public class OutboxEventConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OutboxEventConsumer> _logger;

    public OutboxEventConsumer(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OutboxEventConsumer> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken ct)
    {
        // Consume the events from the message broker
        // check for idempotency of data
        // Insert data into the read database

    }
}