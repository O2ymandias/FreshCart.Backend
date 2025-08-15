using AutoMapper;
using ECommerce.Core.Common.Constants;
using ECommerce.Core.Common.Enums;
using ECommerce.Core.Common.Pagination;
using ECommerce.Core.Common.SpecsParams;
using ECommerce.Core.Dtos.OrderDtos;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Interfaces.Services;
using ECommerce.Core.Models.OrderModule;
using ECommerce.Core.Models.OrderModule.Owned;
using ECommerce.Core.Models.ProductModule;
using ECommerce.Core.Specifications.OrderSpecifications;
using ECommerce.Core.Specifications.ProductSpecifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;

namespace ECommerce.Application.Services;
public class OrderService : IOrderService
{
	private readonly ICartService _cartService;
	private readonly IProductService _productService;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMapper _mapper;
	private readonly IConfiguration _config;
	private readonly IStringLocalizer<OrderService> _localizer;
	private readonly ICultureService _cultureService;

	public OrderService(
		ICartService cartService,
		IProductService productService,
		IUnitOfWork unitOfWork,
		IMapper mapper,
		IConfiguration config,
		IStringLocalizer<OrderService> localizer,
		ICultureService cultureService
		)
	{
		_cartService = cartService;
		_productService = productService;
		_unitOfWork = unitOfWork;
		_mapper = mapper;
		_config = config;
		_localizer = localizer;
		_cultureService = cultureService;
	}

	public async Task<CreateOrderResult> CreateOrderAsync(CreateOrderInput input, string userId)
	{
		var cart = await _cartService.GetCartAsync(input.CartId);
		if (cart is null)
			return new CreateOrderResult() { Message = _localizer[L.Order.CartNotFound] };

		if (cart.Items.Count == 0)
			return new CreateOrderResult() { Message = _localizer[L.Order.CartEmpty] };

		var deliveryMethod = await _unitOfWork
			.Repository<DeliveryMethod>()
			.GetAsync(new DeliveryMethodSpecs(input.DeliveryMethodId));

		if (deliveryMethod is null)
			return new CreateOrderResult() { Message = _localizer[L.Order.DeliveryMethodUnavailable] };

		decimal subTotal = default;
		List<OrderItem> orderItems = [];

		// BEGIN TRANSACTION
		await _unitOfWork.BeginTransactionAsync();
		try
		{
			foreach (var cartItem in cart.Items)
			{
				var productSpecs = new ProductSpecs(
					specsParams: new() { ProductId = cartItem.ProductId },
					enablePagination: false,
					enableSorting: false,
					enableTracking: true,
					enableSplittingQuery: true
					);

				productSpecs.IncludeRelatedData(p => p.Translations);

				var product = await _unitOfWork
					.Repository<Product>()
					.GetAsync(productSpecs, checkLocalCache: false)
					?? throw new ArgumentException(_localizer[L.Order.ProductNotExists, cartItem.ProductName]);


				var maxOrderQty = await _productService.GetMaxOrderQuantityAsync(cartItem.ProductId);

				if (cartItem.Quantity > maxOrderQty)
					throw new ArgumentException(_localizer[L.Order.NotEnoughStock, product.Name]);


				product.UnitsInStock -= cartItem.Quantity;

				subTotal += product.Price * cartItem.Quantity;

				orderItems.Add(new OrderItem()
				{
					Product = new ProductItem()
					{
						Id = product.Id,
						Name = product.Name,
						PictureUrl = $"{_config["BaseUrl"]}/{product.PictureUrl}",
						NameTranslations = product.Translations.ToDictionary(x => x.LanguageCode.ToString(), x => x.Name)
					},
					Price = product.Price,
					Quantity = cartItem.Quantity
				});
			}

			var order = new Order()
			{
				UserId = userId,
				DeliveryMethodId = input.DeliveryMethodId,
				ShippingAddress = _mapper.Map<ShippingAddress>(input.ShippingAddress),
				PaymentMethod = input.PaymentMethod,
				SubTotal = subTotal,
				Items = orderItems,
			};

			_unitOfWork.Repository<Order>().Add(order);

			// COMMIT TRANSACTION
			await _unitOfWork.CommitTransactionAsync();

			await _cartService.DeleteCartAsync(input.CartId);
			return new CreateOrderResult()
			{
				Success = true,
				Message = _localizer[L.Order.CreatedSuccessfully],
				CreatedOrderId = order.Id
			};
		}
		catch (Exception ex)
		{
			await _unitOfWork.RollbackTransactionAsync();
			return new CreateOrderResult
			{
				Success = false,
				Message = ex.Message
			};
		}
	}

	public async Task<IReadOnlyList<DeliveryMethodResult>> GetDeliveryMethods()
	{
		var canTranslate = _cultureService.CanTranslate;
		DeliveryMethodSpecs deliveryMethodSpecs = new(null);
		if (canTranslate) deliveryMethodSpecs.IncludeRelatedData(x => x.Translations);

		var deliveryMethods = await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync(deliveryMethodSpecs);
		return _mapper.Map<IReadOnlyList<DeliveryMethodResult>>(deliveryMethods);
	}

	public async Task<PaginationResult<OrderResult>> GetOrdersAsync(OrderSpecsParams specsParams)
	{
		var orderSpecs = new OrderSpecs(
			specsParams: specsParams,
			enablePagination: true,
			enableSorting: true,
			enableTracking: false
			);
		orderSpecs.IncludeRelatedData(o => o.Items, o => o.DeliveryMethod);
		var orders = await _unitOfWork
			.Repository<Order>()
			.GetAllAsync(orderSpecs);

		var countSpecs = new OrderSpecs(
			specsParams: specsParams,
			enablePagination: false,
			enableSorting: false,
			enableTracking: false
			);
		var count = await _unitOfWork
			.Repository<Order>()
			.CountAsync(countSpecs);

		return new()
		{
			PageNumber = specsParams.PageNumber,
			PageSize = specsParams.PageSize,
			Total = count,
			Results = _mapper.Map<IReadOnlyList<OrderResult>>(orders)
		};
	}

	public async Task<OrderCancelationResult> CancelOrderAsync(int orderId, string userId)
	{
		var specs = new OrderSpecs(
			specsParams: new() { OrderId = orderId, UserId = userId },
			enablePagination: false,
			enableSorting: false,
			enableTracking: true
			);

		specs.IncludeRelatedData(o => o.Items);

		var order = await _unitOfWork
			.Repository<Order>()
			.GetAsync(specs, checkLocalCache: false);

		if (order is null || !order.IsCancellable)
			return new() { Message = _localizer[L.Order.notCancellable] };

		// BEGIN TRANSACTION
		await _unitOfWork.BeginTransactionAsync();
		try
		{
			foreach (var orderItem in order.Items)
			{
				var productSpecs = new ProductSpecs(
					specsParams: new() { ProductId = orderItem.Product.Id },
					enablePagination: false,
					enableSorting: false,
					enableTracking: true,
					enableSplittingQuery: false
					);

				var product = await _unitOfWork
					.Repository<Product>()
					.GetAsync(productSpecs, false)
					?? throw new ArgumentException(_localizer[L.Order.noProductAssociated]);

				product.UnitsInStock += orderItem.Quantity;
			}

			order.OrderStatus = OrderStatus.Cancelled;
			order.PaymentStatus = PaymentStatus.PaymentFailed;

			// COMMIT TRANSACTION
			await _unitOfWork.CommitTransactionAsync();

			return new()
			{
				ManageToCancel = true,
				CheckoutSessionId = order.CheckoutSessionId,
				Message = _localizer[L.Order.cancelledSuccessfully]
			};
		}
		catch (Exception ex)
		{
			// ROLLBACK TRANSACTION
			await _unitOfWork.RollbackTransactionAsync();
			return new() { Message = _localizer[L.Order.cancelFailed, ex.Message] };
		}
	}

	public async Task<LatestOrdersResult> GetLatestOrdersAsync(string userId, int limit)
	{
		var ordersSpecsParams = new OrderSpecsParams()
		{
			UserId = userId,
			PageNumber = 1,
			PageSize = limit,
			Sort = new()
			{
				Key = OrderSortKey.CreatedAt,
				Dir = SortDirection.Desc,
			}
		};
		var ordersSpecs = new OrderSpecs(
			specsParams: ordersSpecsParams,
			enablePagination: true,
			enableSorting: true,
			enableTracking: false
			);
		ordersSpecs.IncludeRelatedData(o => o.Items, o => o.DeliveryMethod);
		var orders = await _unitOfWork
			.Repository<Order>()
			.GetAllAsync(ordersSpecs);

		var countSpecs = new OrderSpecs(
			specsParams: new() { UserId = userId },
			enablePagination: false,
			enableSorting: false,
			enableTracking: false
			);

		var count = await _unitOfWork
			.Repository<Order>()
			.CountAsync(countSpecs);

		return new()
		{
			LatestOrders = _mapper.Map<IReadOnlyList<OrderResult>>(orders),
			Count = count,
		};
	}
}
