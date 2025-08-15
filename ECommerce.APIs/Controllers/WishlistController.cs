using ECommerce.APIs.ErrorModels;
using ECommerce.Core.Common.Pagination;
using ECommerce.Core.Common.SpecsParams;
using ECommerce.Core.Dtos.WishlistDtos;
using ECommerce.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.APIs.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WishlistController : ControllerBase
{
	private readonly IWishlistService _wishlistService;
	private readonly IApiErrorResponseFactory _errorFactory;

	public WishlistController(IWishlistService wishlistService, IApiErrorResponseFactory errorFactory)
	{
		_wishlistService = wishlistService;
		_errorFactory = errorFactory;
	}

	[HttpGet]
	[ProducesResponseType(typeof(PaginationResult<WishlistItemResult>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<PaginationResult<WishlistItemResult>>> GetUserWishListItems(
		[FromQuery] WishlistItemSpecsParams specsParams
		)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}

		specsParams.UserId = userId;

		return Ok(await _wishlistService.GetUserWishListItemsAsync(specsParams));
	}

	[HttpGet("product-ids")]
	[ProducesResponseType(typeof(IReadOnlyList<int>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<IReadOnlyList<int>>> GetWishListProductIds()
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}

		return Ok(await _wishlistService.GetWishlistProductIdsAsync(userId));
	}

	[HttpGet("total")]
	[ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<int>> GetTotal()
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}

		return Ok(await _wishlistService.GetTotalAsync(userId));
	}

	[HttpPost("add-to-wishlist")]
	[ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<bool>> AddToWishlist([FromBody] WishlistItemInput input)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}
		return Ok(await _wishlistService.AddToWishListAsync(input, userId));
	}

	[HttpDelete("remove-from-wishlist")]
	[ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<bool>> RemoveFromWishlist([FromBody] WishlistItemInput input)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}

		return Ok(await _wishlistService.DeleteFromWishlistAsync(input, userId));
	}

	[HttpDelete("clear-wishlist")]
	[ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<bool>> ClearWishlist()
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}
		return Ok(await _wishlistService.ClearWishlistAsync(userId));
	}
}