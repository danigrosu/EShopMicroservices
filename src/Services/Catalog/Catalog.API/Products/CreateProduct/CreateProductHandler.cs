using BuildingBlocks.CQRS;
using Catalog.API.Model;

namespace Catalog.API.Products.CreateProduct
{
    public record CreateProductCommand(
        string Name,
        string Description,
        List<string> Category,
        string ImageFile,
        decimal Price) : ICommand<CreateProductResult>;

    public record CreateProductResult(Guid Id);

    internal class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductResult>
    {
        public Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
        {
            // create Product enrity from command object
            // save to db
            // return CreateProductResult result

            var product = new Product
            { 
                Name = command.Name,
                Description = command.Description,
                Category = command.Category,
                ImageFile = command.ImageFile,
                Price = command.Price
            };

            return Task.FromResult(new CreateProductResult(Guid.NewGuid()));
        }
    }
}
