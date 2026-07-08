namespace Shared.Messaging;

public static class RabbitMqConstants
{
    public const string OrdersExchange = "orders.exchange";
    public const string OrdersQueue = "orders.queue";
    public const string OrderCreatedRoutingKey = "order.created";

    public const string OrdersDeadLetterExchange = "orders.dlx.exchange";
    public const string OrdersDeadLetterQueue = "orders.deadletter.queue";
    public const string OrdersDeadLetterRoutingKey = "order.dead";
}