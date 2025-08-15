namespace ECommerce.Core.Dtos.OrderDtos;
public class LatestOrdersResult
{
	public int Count { get; set; }
	public IReadOnlyList<OrderResult> LatestOrders { get; set; }
}
