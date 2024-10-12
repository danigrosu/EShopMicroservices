namespace Ordering.Application.Exceptions;

public class OrderNotFoundException(string message) : NotFoundException(message)
{
}