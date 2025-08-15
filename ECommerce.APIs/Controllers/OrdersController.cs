using ECommerce.APIs.ErrorModels;
using ECommerce.Core.Common.Pagination;
using ECommerce.Core.Common.SpecsParams;
using ECommerce.Core.Dtos.OrderDtos;
using ECommerce.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.APIs.Controllers;
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
	private readonly IOrderService _orderService;
	private readonly ICheckoutService _checkoutService;
	private readonly IApiErrorResponseFactory _errorFactory;

	public OrdersController(IOrderService orderService,
		ICheckoutService checkoutService,
		IApiErrorResponseFactory errorFactory
		)
	{
		_orderService = orderService;
		_checkoutService = checkoutService;
		_errorFactory = errorFactory;
	}

	[HttpPost]
	[ProducesResponseType(typeof(CreateOrderResult), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<CreateOrderResult>> CreateOrder([FromBody] CreateOrderInput input)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}

		var result = await _orderService.CreateOrderAsync(input, userId);

		return result.Success
			? Ok(result)
			: BadRequest(_errorFactory.CreateErrorResponse(StatusCodes.Status400BadRequest, result.Message));
	}

	[HttpGet]
	[ProducesResponseType(typeof(PaginationResult<OrderResult>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<PaginationResult<OrderResult>>> GetOrders([FromQuery] OrderSpecsParams specsParams)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}

		specsParams.UserId = userId;

		return Ok(await _orderService.GetOrdersAsync(specsParams));
	}

	[HttpGet("latest")]
	[ProducesResponseType(typeof(LatestOrdersResult), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<LatestOrdersResult>> GetLatestOrders(int limit)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}

		return Ok(await _orderService.GetLatestOrdersAsync(userId, limit));
	}

	[HttpDelete("{orderId}")]
	[ProducesResponseType(typeof(CancelOrderResult), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<CancelOrderResult>> CancelOrder(int orderId)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}

		var cancelResult = await _orderService.CancelOrderAsync(orderId, userId);

		if (!cancelResult.ManageToCancel)
			return BadRequest(_errorFactory.CreateErrorResponse(StatusCodes.Status400BadRequest, cancelResult.Message));

		var response = new CancelOrderResult
		{
			ManageToCancelOrder = true,
			CancelMessage = cancelResult.Message
		};

		if (!string.IsNullOrEmpty(cancelResult.CheckoutSessionId))
		{
			var expiredResult = await _checkoutService.ExpireCheckoutSessionAsync(cancelResult.CheckoutSessionId);
			response.ManageToExpireSession = expiredResult.ManageToExpire;
			response.ExpireMessage = expiredResult.Message;
		}

		return Ok(response);
	}

	[AllowAnonymous]
	[HttpGet("delivery-methods")]
	[ProducesResponseType(typeof(IReadOnlyList<DeliveryMethodResult>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetDeliveryMethods() =>
		Ok(await _orderService.GetDeliveryMethods());
}
