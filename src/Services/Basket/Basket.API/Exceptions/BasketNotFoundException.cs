namespace Basket.API.Exceptions;

public class BasketNotFoundException(string message) : NotFoundException("basket", message)
{
}
