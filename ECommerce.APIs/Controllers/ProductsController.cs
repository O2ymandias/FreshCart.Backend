using ECommerce.APIs.ErrorModels;
using ECommerce.APIs.Filters;
using ECommerce.Core.Common.Pagination;
using ECommerce.Core.Common.SpecsParams;
using ECommerce.Core.Dtos.ProductDtos;
using ECommerce.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.APIs.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
	private readonly IProductService _productService;
	private readonly IApiErrorResponseFactory _errorFactory;

	public ProductsController(IProductService productService, IApiErrorResponseFactory errorFactory)
	{
		_productService = productService;
		_errorFactory = errorFactory;
	}

	[TypeFilter(typeof(CacheAttribute<PaginationResult<ProductResult>>), Arguments = [15])]
	[HttpGet]
	[ProducesResponseType(typeof(PaginationResult<ProductResult>), StatusCodes.Status200OK)]
	public async Task<ActionResult<PaginationResult<ProductResult>>> GetAll([FromQuery] ProductSpecsParams specsParams) =>
		Ok(await _productService.GetAllProductsWithCountAsync(specsParams));

	[HttpGet("{id}")]
	[ProducesResponseType(typeof(ProductResult), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ProductResult>> Get([FromRoute] int id)
	{
		var product = await _productService.GetProductByIdAsync(id);
		return product is null
			? NotFound(_errorFactory.CreateErrorResponse(StatusCodes.Status404NotFound))
			: Ok(product);
	}

	[HttpGet("brands")]
	[ProducesResponseType(typeof(IReadOnlyList<BrandResult>), StatusCodes.Status200OK)]
	public async Task<ActionResult<IReadOnlyList<BrandResult>>> GetAllBrands() =>
		Ok(await _productService.GetAllBrandsAsync());

	[HttpGet("categories")]
	[ProducesResponseType(typeof(IReadOnlyList<CategoryResult>), StatusCodes.Status200OK)]
	public async Task<ActionResult<IReadOnlyList<CategoryResult>>> GetAllCategories() =>
		Ok(await _productService.GetAllCategoriesAsync());

	[HttpGet("gallery/{productId}")]
	[ProducesResponseType(typeof(IReadOnlyList<ProductGalleryResult>), StatusCodes.Status200OK)]
	public async Task<ActionResult<IReadOnlyList<ProductGalleryResult>>> GetProductGallery(int productId) =>
		Ok(await _productService.GetProductGalleryAsync(productId));

	[HttpGet("{productId}/max-order-quantity")]
	[ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
	public async Task<ActionResult<int>> GetMaxOrderQuantity(int productId) =>
		Ok(await _productService.GetMaxOrderQuantityAsync(productId));

}
