using System.Text.Json;

namespace Basket.API.Outbox;

/// <summary>
/// Background relay that publishes pending outbox messages to RabbitMQ.
///
/// Delivery is at-least-once, not exactly-once: if the process stops after the broker
/// confirms a publish but before the message is marked as processed, the message is
/// published again on the next run. Consumers must therefore be idempotent.
/// </summary>
public class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    IBus bus,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan PublishTimeout = TimeSpan.FromSeconds(10);
    private const int BatchSize = 20;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingMessages(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                // Transient failure (broker or database unavailable). Nothing was marked
                // as processed, so the same messages are picked up on the next poll.
                logger.LogWarning(ex, "Outbox processing failed; retrying in {Interval}", PollInterval);
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task PublishPendingMessages(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();

        var messages = await session.Query<OutboxMessage>()
            .Where(m => m.ProcessedOn == null && m.Error == null)
            .OrderBy(m => m.OccurredOn)
            .Take(BatchSize)
            .ToListAsync(stoppingToken);

        foreach (var message in messages)
        {
            if (!TryDeserialize(message, out var payload, out var messageType))
            {
                // Permanent failure: retrying will never help, so park the message.
                session.Store(message);
                await session.SaveChangesAsync(stoppingToken);
                continue;
            }

            // MassTransit waits for RabbitMQ's publisher confirm, so when this returns the
            // broker has the message. If the broker is down this throws, the batch stops,
            // and the message stays pending.
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            timeout.CancelAfter(PublishTimeout);
            await bus.Publish(payload, messageType, timeout.Token);

            message.ProcessedOn = DateTime.UtcNow;
            session.Store(message);
            await session.SaveChangesAsync(stoppingToken);

            logger.LogInformation("Published outbox message {MessageId} ({MessageType})", message.Id, messageType.Name);
        }
    }

    private bool TryDeserialize(OutboxMessage message, out object payload, out Type messageType)
    {
        payload = default!;
        messageType = default!;

        try
        {
            messageType = Type.GetType(message.Type)
                ?? throw new InvalidOperationException($"Unknown message type '{message.Type}'.");
            payload = JsonSerializer.Deserialize(message.Payload, messageType)
                ?? throw new InvalidOperationException("Payload deserialized to null.");
            return true;
        }
        catch (Exception ex) when (ex is InvalidOperationException or JsonException)
        {
            message.Error = ex.Message;
            logger.LogError(ex, "Outbox message {MessageId} cannot be published and was parked", message.Id);
            return false;
        }
    }
}
