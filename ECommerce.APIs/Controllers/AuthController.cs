using ECommerce.APIs.ErrorModels;
using ECommerce.Core.Dtos.AuthDtos;
using ECommerce.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.APIs.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
	private readonly IAuthService _authService;
	private readonly IApiErrorResponseFactory _errorFactory;

	public AuthController(IAuthService authService, IApiErrorResponseFactory errorFactory)
	{
		_authService = authService;
		_errorFactory = errorFactory;
	}

	[HttpPost("register")]
	[ProducesResponseType(typeof(AuthResult), StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
	public async Task<ActionResult<AuthResult>> Register(RegisterInput register)
	{
		var result = await _authService.RegisterUserAsync(register);

		if (result.Status == StatusCodes.Status201Created)
		{
			SetRefreshTokenInCookies(result.RefreshToken, result.RefreshTokenExpiresOn);
			return StatusCode(result.Status, result);
		}

		return result.Status switch
		{
			StatusCodes.Status400BadRequest => BadRequest(_errorFactory.CreateValidationErrorResponse(result.Errors)),
			StatusCodes.Status409Conflict => Conflict(_errorFactory.CreateErrorResponse(result.Status, result.Message)),
			_ => StatusCode(result.Status)
		};
	}

	[HttpPost("login")]
	[ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<AuthResult>> Login(LoginInput loginDto)
	{
		var result = await _authService.LoginUserAsync(loginDto);

		if (result.Status == StatusCodes.Status200OK)
		{
			SetRefreshTokenInCookies(result.RefreshToken, result.RefreshTokenExpiresOn);
			return Ok(result);
		}

		return Unauthorized(_errorFactory.CreateErrorResponse(result.Status, result.Message));
	}

	[HttpGet("refresh-token")]
	[ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<AuthResult>> RefreshToken()
	{
		var refreshToken = Request.Cookies["refreshToken"];

		var result = await _authService.RefreshTokenAsync(refreshToken);

		if (result.Status == StatusCodes.Status200OK)
		{
			SetRefreshTokenInCookies(result.RefreshToken, result.RefreshTokenExpiresOn);
			return Ok(result);
		}

		return result.Status switch
		{
			StatusCodes.Status400BadRequest =>
			BadRequest(_errorFactory.CreateErrorResponse(result.Status, result.Message)),

			StatusCodes.Status401Unauthorized =>
			Unauthorized(_errorFactory.CreateErrorResponse(result.Status, result.Message)),

			_ => StatusCode(result.Status)
		};
	}

	[HttpGet("revoke-token")]
	[ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
	public async Task<ActionResult<bool>> RevokeToken()
	{
		var refreshToken = Request.Cookies["refreshToken"];
		var result = await _authService.RevokeTokenAsync(refreshToken);
		return Ok(result);
	}

	private void SetRefreshTokenInCookies(string refreshToken, DateTime expiresOn)
	{
		Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions()
		{
			HttpOnly = true,
			Expires = expiresOn.ToLocalTime(),
		});
	}

}
