using AutoMapper;
using ECommerce.Core.Common.Constants;
using ECommerce.Core.Dtos.CartDtos;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Interfaces.Repositories;
using ECommerce.Core.Interfaces.Services;
using ECommerce.Core.Models.CartModule;
using ECommerce.Core.Models.ProductModule;
using ECommerce.Core.Specifications.ProductSpecifications;
using Microsoft.Extensions.Localization;

namespace ECommerce.Application.Services;
public class CartService : ICartService
{
	private readonly IRedisRepository _redisRepo;
	private readonly IProductService _productService;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IStringLocalizer<CartService> _localizer;
	private readonly IMapper _mapper;

	public CartService(IRedisRepository redisRepo,
		IProductService productService,
		IUnitOfWork unitOfWork,
		IStringLocalizer<CartService> localizer,
		IMapper mapper
		)
	{
		_redisRepo = redisRepo;
		_productService = productService;
		_unitOfWork = unitOfWork;
		_localizer = localizer;
		_mapper = mapper;
	}

	public async Task<CartResult?> GetCartAsync(string cartId)
	{
		var cart = await _redisRepo.GetAsync<Cart>(cartId);
		return cart is not null
			? _mapper.Map<CartResult>(cart)
			: null;
	}

	public async Task<CartUpdateResult> AddToCartAsync(CartItemInput item)
	{
		var cart = await _redisRepo.GetAsync<Cart>(item.CartId) ?? new Cart(item.CartId);

		var productSpecs = new ProductSpecs(
			specsParams: new() { ProductId = item.ProductId },
			enablePagination: false,
			enableSorting: false,
			enableTracking: false,
			enableSplittingQuery: true
			);

		productSpecs.IncludeRelatedData(p => p.Translations);

		var product = await _unitOfWork
			.Repository<Product>()
			.GetAsync(productSpecs);

		var result = new CartUpdateResult();

		if (product is null)
		{
			result.Updated = false;
			result.Message = _localizer[L.Cart.ProductNotFound];
			return result;
		}

		if (product.UnitsInStock == 0)
		{
			result.Updated = false;
			result.Message = _localizer[L.Cart.OutOfStock, item.ProductId];
			return result;
		}

		var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
		if (existingItem is not null)
		{
			var maxQty = await _productService.GetMaxOrderQuantityAsync(item.ProductId);
			existingItem.Quantity = Math.Min(existingItem.Quantity + 1, maxQty);

			result.Message = _localizer[L.Cart.IncreaseQuantity, item.ProductId, existingItem.Quantity];
		}
		else
		{
			cart.Items.Add(new CartItem()
			{
				ProductId = product.Id,
				ProductName = product.Name,
				ProductPictureUrl = product.PictureUrl,
				ProductPrice = product.Price,
				Quantity = 1,

				NameTranslations = product.Translations.ToDictionary(x => x.LanguageCode.ToString(), x => x.Name)
			});

			result.Message = _localizer[L.Cart.AddSuccess, item.ProductId];
		}

		var isSet = await _redisRepo.SetAsync(cart.Id, cart);

		if (isSet)
		{
			result.Updated = true;
			return result;
		}
		else
		{
			result.Updated = false;
			result.Message = _localizer[L.Cart.AddFailed, item.ProductId];
			return result;
		}
	}

	public async Task<CartUpdateResult> RemoveFromCartAsync(CartItemInput item)
	{
		var result = new CartUpdateResult();

		var cart = await _redisRepo.GetAsync<Cart>(item.CartId);

		if (cart is null)
		{
			result.Updated = false;
			result.Message = _localizer[L.Cart.CartNotFound];
			return result;
		}

		if (!cart.Items.Any(i => i.ProductId == item.ProductId))
		{
			result.Updated = false;
			result.Message = _localizer[L.Cart.ProductNotInCart, item.ProductId];
			return result;
		}

		cart.Items.RemoveAll(i => i.ProductId == item.ProductId);

		var isSet = await _redisRepo.SetAsync(cart.Id, cart);
		if (isSet)
		{
			result.Updated = true;
			result.Message = _localizer[L.Cart.RemoveSuccess, item.ProductId];
			return result;
		}
		else
		{
			result.Updated = false;
			result.Message = _localizer[L.Cart.RemoveFailed, item.ProductId];
			return result;
		}
	}

	public async Task<CartUpdateResult> UpdateQuantityAsync(UpdateQuantityInput item)
	{

		if (item.NewQuantity <= 0) return new() { Message = _localizer[L.Cart.QuantityMustBeGreaterThanZero, item.NewQuantity], };

		var cart = await _redisRepo.GetAsync<Cart>(item.CartId);
		if (cart is null) return new() { Message = _localizer[L.Cart.CartNotFound, item.CartId], };

		var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
		if (existingItem is null) return new() { Message = _localizer[L.Cart.ProductNotInCart, item.ProductId] };

		var maxQty = await _productService.GetMaxOrderQuantityAsync(item.ProductId);

		existingItem.Quantity = Math.Min(item.NewQuantity, maxQty);

		var isSet = await _redisRepo.SetAsync(cart.Id, cart);

		if (isSet) return new()
		{
			Updated = true,
			Message = _localizer[L.Cart.UpdateQuantitySuccess, item.ProductId, existingItem.Quantity]
		};

		return new() { Message = _localizer[L.Cart.UpdateQuantityFailed, item.CartId, item.ProductId] };
	}

	public async Task<bool> DeleteCartAsync(string id) => await _redisRepo.DeleteAsync(id);
}
