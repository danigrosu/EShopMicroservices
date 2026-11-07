namespace Basket.API.Data;

public class BasketRepository(IDocumentSession session) : IBasketRepository
{
    public async Task<ShoppingCart> GetBasket(string userName, CancellationToken cancellationToken = default)
    {
        var basket = await session.LoadAsync<ShoppingCart>(userName, cancellationToken);
        return basket ?? throw new BasketNotFoundException(userName);
    }
    public async Task<ShoppingCart> StoreBasket(ShoppingCart basket, CancellationToken cancellationToken = default)
    {
        session.Store(basket);
        await session.SaveChangesAsync(cancellationToken);
        return basket;
    }

    public async Task<bool> DeleteBasket(string userName, CancellationToken cancellationToken = default)
    {
        session.Delete<ShoppingCart>(userName);
        await session.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task CheckoutBasket(string userName, IntegrationEvent checkoutEvent, CancellationToken cancellationToken = default)
    {
        // Both changes go to the same Postgres database, and Marten commits everything
        // queued on a session in one transaction: either the basket is gone AND the event
        // is queued, or neither happened. OutboxProcessor publishes the event afterwards.
        session.Delete<ShoppingCart>(userName);
        session.Store(OutboxMessage.From(checkoutEvent));
        await session.SaveChangesAsync(cancellationToken);
    }
}
