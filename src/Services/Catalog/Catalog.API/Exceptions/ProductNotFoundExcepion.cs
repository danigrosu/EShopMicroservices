namespace Catalog.API.Exceptions;

public class ProductNotFoundExcepion : NotFoundException
{
    public ProductNotFoundExcepion(Guid Id) : base("Product", Id)
    {
    }
}
