namespace BuildingBlocks.Messaging.Events;

public record IntegrationEvent
{
    // Assigned once when the event is created and kept through serialization.
    // Consumers use it as the idempotency key, so it must never change.
    // (Previously `Id => Guid.NewGuid()`, which returned a different value on every read.)
    public Guid Id { get; init; } = Guid.NewGuid();

    // A property rather than a public field: System.Text.Json skips fields,
    // so the field version never reached the consumer.
    public DateTime OccuredOn { get; init; } = DateTime.UtcNow;

    public string EventType => GetType().AssemblyQualifiedName!;
}
