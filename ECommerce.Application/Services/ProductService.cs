using AutoMapper;
using ECommerce.Core.Common.Enums;
using ECommerce.Core.Common.Options;
using ECommerce.Core.Common.Pagination;
using ECommerce.Core.Common.SpecsParams;
using ECommerce.Core.Dtos.ProductDtos;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Interfaces.Services;
using ECommerce.Core.Models.BrandModule;
using ECommerce.Core.Models.CategoryModule;
using ECommerce.Core.Models.ProductModule;
using ECommerce.Core.Specifications.ProductSpecifications;
using Microsoft.Extensions.Options;

namespace ECommerce.Application.Services;

public class ProductService : IProductService
{
	private readonly CartOptions _cartOptions;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMapper _mapper;
	private readonly ICultureService _cultureService;

	public ProductService(
		IUnitOfWork unitOfWork,
		IMapper mapper,
		IOptions<CartOptions> cartConfigOptions,
		ICultureService cultureService
		)
	{
		_unitOfWork = unitOfWork;
		_mapper = mapper;
		_cultureService = cultureService;
		_cartOptions = cartConfigOptions.Value;
	}

	public async Task<PaginationResult<ProductResult>> GetAllProductsWithCountAsync(ProductSpecsParams specsParams)
	{
		var canTranslate = _cultureService.CanTranslate;

		var productSpecs = new ProductSpecs(
			specsParams: specsParams,
			enablePagination: true,
			enableSorting: true,
			enableTracking: false,
			enableSplittingQuery: canTranslate
			);

		productSpecs.IncludeRelatedData(p => p.Brand, p => p.Category);

		if (canTranslate)
		{
			productSpecs.IncludeRelatedData(p => p.Translations);
			productSpecs.IncludeRelatedData(
				x => x.Brand.Translations,
				x => x.Category.Translations
				);
		}

		var countSpecs = new ProductSpecs(
			specsParams: specsParams,
			enablePagination: false,
			enableSorting: false,
			enableTracking: false,
			enableSplittingQuery: false
			);

		var allProducts = await _unitOfWork
			.Repository<Product>()
			.GetAllAsync(productSpecs);

		var totalCount = await _unitOfWork
			.Repository<Product>()
			.CountAsync(countSpecs);

		return new PaginationResult<ProductResult>()
		{
			PageNumber = specsParams.PageNumber,
			PageSize = specsParams.PageSize,
			Results = _mapper.Map<IReadOnlyList<ProductResult>>(allProducts),
			Total = totalCount
		};
	}

	public async Task<ProductResult?> GetProductByIdAsync(int productId)
	{
		var culture = Thread.CurrentThread.CurrentCulture;
		var shouldTranslate = Enum.TryParse(culture.Name, ignoreCase: true, out LanguageCode lang) && lang is not LanguageCode.EN;

		var productSpecs = new ProductSpecs(
			specsParams: new() { ProductId = productId },
			enablePagination: false,
			enableSorting: false,
			enableTracking: false,
			enableSplittingQuery: shouldTranslate
			);

		productSpecs.IncludeRelatedData(p => p.Brand, p => p.Category);
		if (shouldTranslate)
		{
			productSpecs.IncludeRelatedData(p => p.Translations);
			productSpecs.IncludeRelatedData(
				x => x.Brand.Translations,
				x => x.Category.Translations
				);
		}

		var product = await _unitOfWork.Repository<Product>().GetAsync(productSpecs);

		return product is not null ? _mapper.Map<ProductResult>(product) : null;
	}

	public async Task<IReadOnlyList<BrandResult>> GetAllBrandsAsync()
	{
		var culture = Thread.CurrentThread.CurrentCulture;
		var shouldTranslate = Enum.TryParse(culture.Name, ignoreCase: true, out LanguageCode lang) && lang is not LanguageCode.EN;

		var brandSpecs = new BrandSpecs();
		if (shouldTranslate)
			brandSpecs.IncludeRelatedData(b => b.Translations);

		var brands = await _unitOfWork
			.Repository<Brand>()
			.GetAllAsync(brandSpecs);

		return _mapper.Map<IReadOnlyList<BrandResult>>(brands);
	}

	public async Task<IReadOnlyList<CategoryResult>> GetAllCategoriesAsync()
	{
		var culture = Thread.CurrentThread.CurrentCulture;
		var shouldTranslate = Enum.TryParse(culture.Name, ignoreCase: true, out LanguageCode lang) && lang is not LanguageCode.EN;

		var categorySpecs = new CategorySpecs();

		if (shouldTranslate)
			categorySpecs.IncludeRelatedData(c => c.Translations);

		var categories = await _unitOfWork
			.Repository<Category>()
			.GetAllAsync(categorySpecs);

		return _mapper.Map<IReadOnlyList<CategoryResult>>(categories);
	}

	public async Task<IReadOnlyList<ProductGalleryResult>> GetProductGalleryAsync(int productId)
	{
		var specs = new ProductGallerySpecs(productId);
		var gallery = await _unitOfWork
			.Repository<ProductGallery>()
			.GetAllAsync(specs);
		return _mapper.Map<IReadOnlyList<ProductGalleryResult>>(gallery);
	}

	public async Task<int> GetMaxOrderQuantityAsync(int productId)
	{
		var specs = new ProductSpecs(
			specsParams: new() { ProductId = productId },
			enablePagination: false,
			enableSorting: false,
			enableTracking: false,
			enableSplittingQuery: false
			);

		var product = await _unitOfWork
			.Repository<Product>()
			.GetAsync(specs)
			?? throw new ArgumentException($"There is no product with id `{productId}`");

		var maxOrder = (int)Math.Ceiling(product.UnitsInStock * _cartOptions.MaxOrderRate);
		return Math.Min(maxOrder, _cartOptions.MaxOrderQuantityCap);
	}

}
