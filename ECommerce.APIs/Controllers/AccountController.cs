using ECommerce.APIs.ErrorModels;
using ECommerce.APIs.ResponseModels;
using ECommerce.Core.Dtos.AuthDtos;
using ECommerce.Core.Dtos.ProfileDtos;
using ECommerce.Core.Interfaces.Services;
using ECommerce.Core.Models.OrderModule.Owned;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.APIs.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AccountController : ControllerBase
{
	private readonly IAccountService _accountService;
	private readonly IApiErrorResponseFactory _errorFactory;

	public AccountController(IAccountService accountService, IApiErrorResponseFactory errorFactory)
	{
		_accountService = accountService;
		_errorFactory = errorFactory;
	}

	[HttpGet("user-info")]
	[ProducesResponseType(typeof(UserInfoResult), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<UserInfoResult>> GetUserInfo()
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}

		return Ok(await _accountService.GetUserInfoAsync(userId));
	}

	[HttpGet("address")]
	[ProducesResponseType(typeof(ShippingAddress), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<ShippingAddress>> GetUserShippingAddress()
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}

		return Ok(await _accountService.GetUserShippingAddressAsync(userId));
	}

	[HttpPut("basic-info")]
	[ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<ApiMessageResponse>> UpdateBasicInfo([FromForm] BasicInfoUpdateInput input)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}

		var result = await _accountService.UpdateBasicInfoAsync(input, userId);
		return result.Updated
			? Ok(new ApiMessageResponse() { Message = result.Message ?? string.Empty })
			: BadRequest(_errorFactory.CreateErrorResponse(StatusCodes.Status400BadRequest, result.Message));
	}

	[AllowAnonymous]
	[HttpPost("forget-password")]
	[ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status200OK)]
	public async Task<ActionResult<ApiMessageResponse>> ForgetPassword(ForgetPasswordInput forgetPassword)
	{
		var message = await _accountService.ForgetPasswordAsync(forgetPassword);
		return Ok(new ApiMessageResponse() { Message = message });
	}

	[AllowAnonymous]
	[HttpPost("reset-password")]
	[ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<ApiMessageResponse>> ResetPassword(PasswordResetInput resetPassword)
	{
		var result = await _accountService.ResetPasswordAsync(resetPassword);
		return result.Status switch
		{
			StatusCodes.Status200OK => Ok(new ApiMessageResponse() { Message = result.Message }),

			StatusCodes.Status400BadRequest =>
			BadRequest(_errorFactory.CreateErrorResponse(result.Status, result.Message)),

			_ => StatusCode(result.Status)
		};
	}

	[HttpPut("change-password")]
	[ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<ApiMessageResponse>> ChangePasswordAsync(PasswordChangeInput input)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}

		var result = await _accountService.ChangePasswordAsync(input, userId);
		return result.Updated
			? Ok(new ApiMessageResponse() { Message = result.Message ?? string.Empty })
			: BadRequest(_errorFactory.CreateErrorResponse(StatusCodes.Status400BadRequest, result.Message));
	}

	[HttpPost("request-email-change")]
	[ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiMessageResponse>> RequestEmailChange(EmailChangeInput input)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}

		var (status, message) = await _accountService.RequestEmailChangeAsync(userId, input);

		return status switch
		{
			StatusCodes.Status200OK => Ok(new ApiMessageResponse() { Message = message }),
			StatusCodes.Status400BadRequest => BadRequest(_errorFactory.CreateErrorResponse(status, message)),
			StatusCodes.Status404NotFound => NotFound(_errorFactory.CreateErrorResponse(status, message)),
			_ => StatusCode(status)
		};
	}


	[HttpPost("confirm-email-change")]
	[ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiMessageResponse>> ConfirmEmailChange(EmailChangeVerificationCodeInput input)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
		{
			var error = _errorFactory.CreateErrorResponse(StatusCodes.Status401Unauthorized);
			return Unauthorized(error);
		}

		var (status, message) = await _accountService.ConfirmEmailChangeAsync(userId, input.VerificationCode);
		return status switch
		{
			StatusCodes.Status200OK => Ok(new ApiMessageResponse() { Message = message }),
			StatusCodes.Status400BadRequest => BadRequest(new ApiErrorResponse(status, message)),
			StatusCodes.Status404NotFound => NotFound(new ApiErrorResponse(status, message)),
			_ => StatusCode(status)
		};
	}
}
