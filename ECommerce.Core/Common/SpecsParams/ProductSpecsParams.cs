using ECommerce.Core.Common.Constants;

namespace ECommerce.Core.Common.SpecsParams;
public class ProductSpecsParams
{
	private int _pageNumber = PaginationConstants.DefaultPageIndex;
	private int _pageSize = PaginationConstants.DefaultPageSize;
	private string? _search;
	private int? _minPrice;
	private int? maxPrice;

	public int? ProductId { get; set; }
	public int? CategoryId { get; set; }
	public int? BrandId { get; set; }
	public ProductSortOptions Sort { get; set; } = new();

	public string? Search
	{
		get => _search;
		set => _search = value?.Trim();
	}

	public int PageNumber
	{
		get => _pageNumber;
		set => _pageNumber = value > 0
			? value
			: PaginationConstants.DefaultPageIndex;
	}
	public int PageSize
	{
		get => _pageSize;
		set => _pageSize = value > 0 && value <= PaginationConstants.MaxPageSize
			? value
			: PaginationConstants.DefaultPageSize;
	}

	public int? MinPrice
	{
		get => _minPrice;
		set => _minPrice = value <= 0 ? null : value;
	}

	public int? MaxPrice
	{
		get => maxPrice;
		set => maxPrice = value <= 0 ? null : value;
	}
}
