using ECommerce.Core.Common.Pagination;
using ECommerce.Core.Common.SpecsParams;
using ECommerce.Core.Dtos.ProductDtos;

namespace ECommerce.Core.Interfaces.Services;
public interface IProductService
{
	Task<PaginationResult<ProductResult>> GetAllProductsWithCountAsync(ProductSpecsParams specsParams);
	Task<ProductResult?> GetProductByIdAsync(int productId);
	Task<IReadOnlyList<BrandResult>> GetAllBrandsAsync();
	Task<IReadOnlyList<CategoryResult>> GetAllCategoriesAsync();
	Task<IReadOnlyList<ProductGalleryResult>> GetProductGalleryAsync(int productId);
	Task<int> GetMaxOrderQuantityAsync(int productId);
}
