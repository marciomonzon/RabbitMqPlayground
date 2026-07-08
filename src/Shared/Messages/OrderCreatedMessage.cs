namespace Shared.Messages;

public sealed record OrderCreatedMessage(
    Guid Id,
    string Customer,
    decimal Total,
    DateTime CreatedAt);