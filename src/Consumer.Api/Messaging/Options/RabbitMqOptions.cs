namespace Consumer.Api.Messaging.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; init; } = string.Empty;

    public int Port { get; init; }

    public string User { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}