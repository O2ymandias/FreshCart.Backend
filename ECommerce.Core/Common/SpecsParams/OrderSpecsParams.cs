using ECommerce.Core.Common.Enums;

namespace ECommerce.Core.Common.SpecsParams;
public class OrderSpecsParams : BaseSpecsParams
{
	public int? OrderId { get; set; }
	public string? UserId { get; set; }
	public OrderSortOptions Sort { get; set; } = new();
	public OrderStatus? OrderStatus { get; set; }
}
