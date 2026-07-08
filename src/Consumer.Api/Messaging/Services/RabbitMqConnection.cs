using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Consumer.Api.Messaging.Interfaces;
using Consumer.Api.Messaging.Options;

namespace Consumer.Api.Messaging.Services;

public sealed class RabbitMqConnection : IRabbitMqConnection
{
    private readonly ConnectionFactory _factory;

    private IConnection? _connection;


    public RabbitMqConnection(
        IOptions<RabbitMqOptions> options)
    {
        var settings = options.Value;

        _factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            UserName = settings.User,
            Password = settings.Password
        };
    }


    private async Task<IConnection> GetConnectionAsync(
        CancellationToken cancellationToken)
    {
        if (_connection is not null &&
            _connection.IsOpen)
        {
            return _connection;
        }


        _connection =
            await _factory.CreateConnectionAsync(
                cancellationToken);


        return _connection;
    }


    public async Task<IChannel> CreateChannelAsync(
        CancellationToken cancellationToken = default)
    {
        var connection = await GetConnectionAsync(cancellationToken);

        return await connection.CreateChannelAsync(
            new CreateChannelOptions(
                publisherConfirmationsEnabled: false,
                publisherConfirmationTrackingEnabled: false,
                outstandingPublisherConfirmationsRateLimiter: null,
                consumerDispatchConcurrency: null),
            cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}