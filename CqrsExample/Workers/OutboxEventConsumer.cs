using System.Text;
using System.Text.Json;
using CqrsExample.Documents;
using CqrsExample.Entities;
using CqrsExample.Features.Products.Abstractions;
using CqrsExample.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CqrsExample.Workers;

public class OutboxEventConsumer : BackgroundService
{
    private const string QueueName = "outbox-events";

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConnection _connection;
    private readonly RabbitmqOptions _options;
    private readonly ILogger<OutboxEventConsumer> _logger;
    private IChannel? _channel;

    public OutboxEventConsumer(
        IServiceScopeFactory serviceScopeFactory,
        IConnection connection,
        IOptions<RabbitmqOptions> options,
        ILogger<OutboxEventConsumer> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _connection = connection;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

        await _channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);

        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);

        await _channel.QueueBindAsync(
            queue: QueueName,
            exchange: _options.ExchangeName,
            routingKey: "#",
            arguments: null,
            cancellationToken: ct);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct);

        await Task.Delay(Timeout.Infinite, ct).ContinueWith(_ => { }, TaskScheduler.Default);
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        var channel = ((AsyncEventingBasicConsumer)sender).Channel;

        try
        {
            var payload = Encoding.UTF8.GetString(args.Body.ToArray());

            await ProcessEventAsync(args.RoutingKey, payload, CancellationToken.None);

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false);
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "Error processing message with routing key {RoutingKey}", args.RoutingKey);
            await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true);
        }
    }

    private async Task ProcessEventAsync(string eventType, string payload, CancellationToken ct)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        var productReadRepository = scope.ServiceProvider
            .GetRequiredService<IProductReadRepository>();

        switch (eventType)
        {
            case "ProductCreated":
                var product = JsonSerializer.Deserialize<Product>(payload)
                    ?? throw new InvalidOperationException($"Could not deserialize payload for event '{eventType}'");

                await productReadRepository.UpsertProductAsync(new ProductDocument
                {
                    Id = product.Id,
                    Title = product.Title,
                    Description = product.Description,
                    Price = product.Price
                }, ct);

                break;

            default:
                _logger.LogWarning("No projection handler registered for event type {EventType}", eventType);
                break;
        }
    }

    public override async Task StopAsync(CancellationToken ct)
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync(ct);
        }

        await base.StopAsync(ct);
    }
}
