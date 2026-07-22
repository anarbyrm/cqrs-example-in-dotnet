using CqrsExample.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CqrsExample.Services;

public class RabbitmqService
{
    private readonly ILogger<RabbitmqService> _logger;
    private readonly IConnection _connection;
    private readonly string _exchangeName;

    public RabbitmqService(
        IConnection connection, 
        IOptions<RabbitmqOptions> options, 
        ILogger<RabbitmqService> logger)
    {
        _connection = connection;
        _exchangeName = options.Value.ExchangeName;
        _logger = logger;
    }

    public async Task PublishEvent(string routingKey, string eventData, CancellationToken ct)
    {
        await using var channel = await _connection.CreateChannelAsync(cancellationToken: ct);

        await channel.ExchangeDeclareAsync(
            exchange: _exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);

        var body = System.Text.Encoding.UTF8.GetBytes(eventData);

        await channel.BasicPublishAsync(
            exchange: _exchangeName,
            routingKey: routingKey,
            body: body,
            cancellationToken: ct);
    }
}