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
    private readonly ILogger<OutboxEventConsumer> _logger;
    private IProductReadRepository? _productReadRepository;
    private IConnection? _connection;
    private IOptions<RabbitmqOptions>? _options;
    private IChannel? _channel;

    public OutboxEventConsumer(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OutboxEventConsumer> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("{Worker} starting", nameof(OutboxEventConsumer));

        try
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();

            _productReadRepository = scope.ServiceProvider.GetRequiredService<IProductReadRepository>();
            _connection = scope.ServiceProvider.GetRequiredService<IConnection>();
            _options = scope.ServiceProvider.GetRequiredService<IOptions<RabbitmqOptions>>();

            _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

            await _channel.ExchangeDeclareAsync(
                exchange: _options.Value.ExchangeName,
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
                exchange: _options.Value.ExchangeName,
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

            _logger.LogInformation("{Worker} started and consuming queue {QueueName}", nameof(OutboxEventConsumer), QueueName);

            await Task.Delay(Timeout.Infinite, ct).ContinueWith(_ => { }, TaskScheduler.Default);
        }
        catch (Exception exc) when (exc is not OperationCanceledException)
        {
            _logger.LogError(exc, "{Worker} failed to start", nameof(OutboxEventConsumer));
            throw;
        }
        finally
        {
            _logger.LogInformation("{Worker} stopped", nameof(OutboxEventConsumer));
        }
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        var channel = ((AsyncEventingBasicConsumer)sender).Channel;

        try
        {
            var payload = Encoding.UTF8.GetString(args.Body.ToArray());

            await ProcessEventAsync(args.RoutingKey, payload, CancellationToken.None);

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false);

            _logger.LogInformation("{Worker} processed message with routing key {RoutingKey}", nameof(OutboxEventConsumer), args.RoutingKey);
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "{Worker} failed to process message with routing key {RoutingKey}", nameof(OutboxEventConsumer), args.RoutingKey);
            await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true);
        }
    }

    private async Task ProcessEventAsync(string eventType, string payload, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(_productReadRepository, nameof(_productReadRepository));

        switch (eventType)
        {
            case "ProductCreated":
                var product = JsonSerializer.Deserialize<Product>(payload)
                    ?? throw new InvalidOperationException($"Could not deserialize payload for event '{eventType}'");

                await _productReadRepository.UpsertProductAsync(new ProductDocument
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
        _logger.LogInformation("{Worker} stopping", nameof(OutboxEventConsumer));

        if (_channel is not null)
        {
            await _channel.CloseAsync(ct);
        }

        await base.StopAsync(ct);
    }
}
