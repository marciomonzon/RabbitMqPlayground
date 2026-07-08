using RabbitMQ.Client;

namespace Producer.Api.Messaging.Interfaces;

public interface IRabbitMqConnection : IAsyncDisposable
{
    Task<IChannel> CreateChannelAsync(
        CancellationToken cancellationToken = default);
}