using System.Text.Json;

namespace Basket.API.Outbox;

/// <summary>
/// An integration event waiting to be published to the message broker.
/// It lives in the same Postgres database as the baskets, so it can be saved
/// in the same transaction as the basket change that produced it.
/// </summary>
public class OutboxMessage
{
    // Same value as the event's Id, so the message can be traced end to end.
    public Guid Id { get; set; }
    public string Type { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTime OccurredOn { get; set; }

    // Set once the broker has confirmed the publish.
    public DateTime? ProcessedOn { get; set; }

    // Set when the message can never be published (e.g. its type no longer exists).
    // Such messages are skipped and need a human to look at them.
    public string? Error { get; set; }

    public static OutboxMessage From(IntegrationEvent integrationEvent) => new()
    {
        Id = integrationEvent.Id,
        Type = integrationEvent.GetType().AssemblyQualifiedName!,
        Payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType()),
        OccurredOn = integrationEvent.OccuredOn
    };
}
