using System.Text;
using System.Text.Json;
using Consumer.Api.Messaging.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Messages;
using Shared.Messaging;

namespace Consumer.Api.BackgroundServices;

public sealed class OrderConsumerService : BackgroundService
{
    private readonly IRabbitMqConnection _rabbitMqConnection;
    private readonly ILogger<OrderConsumerService> _logger;

    public OrderConsumerService(
        IRabbitMqConnection rabbitMqConnection,
        ILogger<OrderConsumerService> logger)
    {
        _rabbitMqConnection = rabbitMqConnection;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var channel =
            await _rabbitMqConnection
                .CreateChannelAsync(stoppingToken);


        await channel.QueueDeclareAsync(
            queue: RabbitMqConstants.OrdersQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);


        var consumer =
            new AsyncEventingBasicConsumer(channel);


        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();

            var json =
                Encoding.UTF8.GetString(body);


            var message =
                JsonSerializer.Deserialize<OrderCreatedMessage>(
                    json);


            if (message is not null)
            {
                _logger.LogInformation(
                    "Pedido recebido: {Customer} - {Total}",
                    message.Customer,
                    message.Total);
            }


            await Task.CompletedTask;
        };


        await channel.BasicConsumeAsync(
            queue: RabbitMqConstants.OrdersQueue,
            autoAck: true,
            consumer: consumer,
            cancellationToken: stoppingToken);


        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }
}