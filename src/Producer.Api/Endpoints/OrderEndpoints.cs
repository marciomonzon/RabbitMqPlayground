using Producer.Api.Messaging.Interfaces;
using Shared.Messages;

namespace Producer.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (
            CreateOrderRequest request,
            IOrderPublisher publisher,
            CancellationToken cancellationToken) =>
        {
            var message = new OrderCreatedMessage(
                Guid.NewGuid(),
                request.Customer,
                request.Total,
                DateTime.UtcNow);

            await publisher.PublishAsync(message, cancellationToken);

            return Results.Accepted($"/orders/{message.Id}", message);
        });

        return app;
    }
}

public sealed record CreateOrderRequest(
    string Customer,
    decimal Total);