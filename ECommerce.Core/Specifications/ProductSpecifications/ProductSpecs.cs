using ECommerce.Core.Common;
using ECommerce.Core.Common.Enums;
using ECommerce.Core.Common.SpecsParams;
using ECommerce.Core.Models.ProductModule;
using System.Linq.Expressions;

namespace ECommerce.Core.Specifications.ProductSpecifications;
public class ProductSpecs : BaseSpecification<Product>
{
	private static readonly Expression<Func<Product, object>> _defaultSort = p => p.Name;
	public ProductSpecs(
		ProductSpecsParams specsParams,
		bool enablePagination /*= true*/,
		bool enableSorting /*= true*/,
		bool enableTracking /*= true*/,
		bool enableSplittingQuery /*= false*/
		)
		: base()
	{
		if (enableSorting) SortBy(specsParams.Sort);
		if (enablePagination) ApplyPagination(specsParams.PageNumber, specsParams.PageSize);
		ApplyFiltration(specsParams);
		if (enableTracking) IsTrackingEnabled = true;
		if (enableSplittingQuery) IsSplitQueryEnabled = true;
	}

	protected void ApplyFiltration(ProductSpecsParams specsParams)
	{
		Criteria = p =>
		(!specsParams.ProductId.HasValue || p.Id == specsParams.ProductId.Value) &&
		(!specsParams.BrandId.HasValue || p.BrandId == specsParams.BrandId.Value) &&
		(!specsParams.CategoryId.HasValue || p.CategoryId == specsParams.CategoryId.Value) &&
		(!specsParams.MinPrice.HasValue || p.Price >= specsParams.MinPrice.Value) &&
		(!specsParams.MaxPrice.HasValue || p.Price <= specsParams.MaxPrice.Value) &&
		(string.IsNullOrWhiteSpace(specsParams.Search) ||
			p.Name.Contains(specsParams.Search) ||
			p.Description.Contains(specsParams.Search) ||
			p.Brand.Name.Contains(specsParams.Search) ||
			p.Category.Name.Contains(specsParams.Search));
	}
	protected void SortBy(ProductSortOptions sortOptions)
	{
		if (sortOptions.Dir == SortDirection.Asc)
			SortAsc = sortOptions.Key switch
			{
				ProductSortKey.Name => p => p.Name,
				ProductSortKey.Price => p => p.Price,
				_ => _defaultSort
			};
		else if (sortOptions.Dir == SortDirection.Desc)
			SortDesc = sortOptions.Key switch
			{
				ProductSortKey.Name => p => p.Name,
				ProductSortKey.Price => p => p.Price,
				_ => _defaultSort,
			};
		else
			SortAsc = _defaultSort;
	}
}
