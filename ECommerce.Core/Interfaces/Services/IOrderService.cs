using ECommerce.Core.Common.Pagination;
using ECommerce.Core.Common.SpecsParams;
using ECommerce.Core.Dtos.OrderDtos;

namespace ECommerce.Core.Interfaces.Services;

public interface IOrderService
{
	Task<CreateOrderResult> CreateOrderAsync(CreateOrderInput input, string userId);
	Task<IReadOnlyList<DeliveryMethodResult>> GetDeliveryMethods();
	Task<PaginationResult<OrderResult>> GetOrdersAsync(OrderSpecsParams specsParams);
	Task<LatestOrdersResult> GetLatestOrdersAsync(string userId, int limit);
	Task<OrderCancelationResult> CancelOrderAsync(int orderId, string userId);
}
