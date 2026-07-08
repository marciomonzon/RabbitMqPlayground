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
            arguments: arguments,
            cancellationToken: stoppingToken);


        var consumer =
            new AsyncEventingBasicConsumer(channel);


        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var body = eventArgs.Body.ToArray();

                var json = Encoding.UTF8.GetString(body);

                var message = JsonSerializer.Deserialize<OrderCreatedMessage>(json);

                if (message is null)
                    throw new InvalidOperationException("Mensagem inválida.");

                _logger.LogInformation(
                    "Processando pedido {Id} do cliente {Customer}",
                    message.Id,
                    message.Customer);

                // Simula um processamento
                await Task.Delay(1000, stoppingToken);

                await channel.BasicAckAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);

                _logger.LogInformation(
                    "Pedido {Id} processado com sucesso.",
                    message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar a mensagem.");

                await channel.BasicNackAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
        };


        await channel.BasicConsumeAsync(
            queue: RabbitMqConstants.OrdersQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);


        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }
}