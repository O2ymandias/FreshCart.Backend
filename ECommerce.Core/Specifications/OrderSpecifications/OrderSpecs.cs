using ECommerce.Core.Common;
using ECommerce.Core.Common.Enums;
using ECommerce.Core.Common.SpecsParams;
using ECommerce.Core.Models.OrderModule;
using System.Linq.Expressions;

namespace ECommerce.Core.Specifications.OrderSpecifications;
public class OrderSpecs : BaseSpecification<Order>
{
	private static readonly Expression<Func<Order, object>> _defaultSort = p => p.OrderDate;
	public OrderSpecs(
		OrderSpecsParams specsParams,
		bool enablePagination = true,
		bool enableSorting = true,
		bool enableTracking = true
		)
	{
		ApplyFiltration(specsParams);
		if (enablePagination) ApplyPagination(specsParams.PageNumber, specsParams.PageSize);
		if (enableSorting) SortBy(specsParams.Sort);
		if (enableTracking) IsTrackingEnabled = true;
	}

	protected void SortBy(OrderSortOptions sortOptions)
	{
		if (sortOptions.Dir == SortDirection.Asc)
		{
			SortAsc = sortOptions.Key switch
			{
				OrderSortKey.CreatedAt => o => o.OrderDate,
				OrderSortKey.Price => o => o.SubTotal,
				_ => _defaultSort
			};
		}
		else if (sortOptions.Dir == SortDirection.Desc)
		{
			SortDesc = sortOptions.Key switch
			{
				OrderSortKey.CreatedAt => o => o.OrderDate,
				OrderSortKey.Price => o => o.SubTotal,
				_ => _defaultSort
			};
		}
		else
			SortDesc = _defaultSort;
	}

	protected void ApplyFiltration(OrderSpecsParams specsParams)
	{
		Criteria = order =>
			(!specsParams.OrderId.HasValue || order.Id == specsParams.OrderId.Value) &&
			(!specsParams.OrderStatus.HasValue || order.OrderStatus == specsParams.OrderStatus) &&
			(string.IsNullOrWhiteSpace(specsParams.UserId) || order.UserId.Equals(specsParams.UserId));
	}
}
