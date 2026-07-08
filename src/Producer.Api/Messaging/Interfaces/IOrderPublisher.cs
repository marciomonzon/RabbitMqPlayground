using Shared.Messages;

namespace Producer.Api.Messaging.Interfaces;

public interface IOrderPublisher
{
    Task PublishAsync(
        OrderCreatedMessage message,
        CancellationToken cancellationToken = default);
}