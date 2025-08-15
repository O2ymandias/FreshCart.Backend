using ECommerce.APIs.ErrorModels;
using ECommerce.Core.Dtos.CheckoutDtos;
using ECommerce.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.APIs.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CheckoutController : ControllerBase
{
	private readonly ICheckoutService _checkoutService;
	private readonly IApiErrorResponseFactory _errorFactory;

	public CheckoutController(ICheckoutService checkoutService, IApiErrorResponseFactory errorFactory)
	{
		_checkoutService = checkoutService;
		_errorFactory = errorFactory;
	}

	[HttpPost("{orderId}")]
	[ProducesResponseType(typeof(CheckoutResult), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<CheckoutResult>> CreateCheckoutSession(int orderId)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}

		var result = await _checkoutService.CheckoutAsync(orderId, userId);

		return result.Success
			? Ok(result)
			: BadRequest(new ApiErrorResponse(StatusCodes.Status400BadRequest, result.Message));
	}

	[HttpGet("{orderId}")]
	[ProducesResponseType(typeof(CheckoutResult), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<CheckoutResult>> RetrieveCheckoutSession(int orderId)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}

		var result = await _checkoutService.RetrieveCheckoutSessionAsync(orderId, userId);
		return result.Success
			? Ok(result)
			: BadRequest(new ApiErrorResponse(StatusCodes.Status400BadRequest, result.Message));
	}

}
