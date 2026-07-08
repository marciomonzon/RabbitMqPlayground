using RabbitMQ.Client;

namespace Consumer.Api.Messaging.Interfaces;

public interface IRabbitMqConnection : IAsyncDisposable
{
    Task<IChannel> CreateChannelAsync(
        CancellationToken cancellationToken = default);
}