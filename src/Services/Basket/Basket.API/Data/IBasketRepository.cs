namespace Basket.API.Data;

public interface IBasketRepository
{
    Task<ShoppingCart> GetBasket(string userName, CancellationToken cancellationToken = default);
    Task<ShoppingCart> StoreBasket(ShoppingCart basket, CancellationToken cancellationToken = default);
    Task<bool> DeleteBasket(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the basket and records the checkout event in the outbox, atomically.
    /// </summary>
    Task CheckoutBasket(string userName, IntegrationEvent checkoutEvent, CancellationToken cancellationToken = default);
}
