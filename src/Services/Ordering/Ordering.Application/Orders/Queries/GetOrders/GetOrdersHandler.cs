namespace Ordering.Application.Orders.Queries.GetOrders;

public class GetOrdersHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrdersQuery, GetOrdersResult>
{
    public async Task<GetOrdersResult> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        var count = await dbContext.Orders.LongCountAsync(cancellationToken);

        var pageSize = query.PaginationRequest.PageSize;
        var pageIndex = query.PaginationRequest.PageIndex;

        var orders = await dbContext
            .Orders
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .OrderBy(o => o.OrderName.Value)
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedResult<OrderDto>(
            pageIndex,
            pageSize,
            count,
            orders.ToOrderDtoList());

        return new GetOrdersResult(paginatedResult);
    }
}