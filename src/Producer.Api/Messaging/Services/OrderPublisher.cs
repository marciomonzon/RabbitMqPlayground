using System.Text;
using System.Text.Json;
using Producer.Api.Messaging.Interfaces;
using RabbitMQ.Client;
using Shared.Messages;
using Shared.Messaging;

namespace Producer.Api.Messaging.Services;

public sealed class OrderPublisher : IOrderPublisher
{
    private readonly IRabbitMqConnection _rabbitMqConnection;

    public OrderPublisher(IRabbitMqConnection rabbitMqConnection)
    {
        _rabbitMqConnection = rabbitMqConnection;
    }

    public async Task PublishAsync(
        OrderCreatedMessage message,
        CancellationToken cancellationToken = default)
    {
        await using var channel =
            await _rabbitMqConnection.CreateChannelAsync(cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqConstants.OrdersExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqConstants.OrdersDeadLetterExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var arguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = RabbitMqConstants.OrdersDeadLetterExchange,

            ["x-dead-letter-routing-key"] = RabbitMqConstants.OrdersDeadLetterRoutingKey
        };

        await channel.QueueDeclareAsync(
            queue: RabbitMqConstants.OrdersQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments, // está com a dlq e saberá para onde enviar caso falhe
            cancellationToken: cancellationToken);
        
        await channel.QueueDeclareAsync(
            queue: RabbitMqConstants.OrdersDeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: RabbitMqConstants.OrdersQueue,
            exchange: RabbitMqConstants.OrdersExchange,
            routingKey: RabbitMqConstants.OrderCreatedRoutingKey,
            cancellationToken: cancellationToken);
        
        await channel.QueueBindAsync(
            queue: RabbitMqConstants.OrdersDeadLetterQueue,
            exchange: RabbitMqConstants.OrdersDeadLetterExchange,
            routingKey: RabbitMqConstants.OrdersDeadLetterRoutingKey,
            cancellationToken: cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync(
            exchange: RabbitMqConstants.OrdersExchange,
            routingKey: RabbitMqConstants.OrderCreatedRoutingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}