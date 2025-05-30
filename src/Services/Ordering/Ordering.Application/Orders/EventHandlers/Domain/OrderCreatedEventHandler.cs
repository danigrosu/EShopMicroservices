namespace Ordering.Application.Orders.EventHandlers.Domain;

public class OrderCreatedEventHandler(
    IPublishEndpoint publishEndpoint,
    IFeatureManager featureManager,
    ILogger<OrderCreatedEventHandler> logger)
    : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain Event handled: {DomainEvent}", nameof(OrderCreatedEvent));

        if (await featureManager.IsEnabledAsync("OrderFullfilment"))
        {
            var orderCreatedEvent = domainEvent.order.ToOrderDto();
            await publishEndpoint.Publish(orderCreatedEvent, cancellationToken);
        }
    }
}