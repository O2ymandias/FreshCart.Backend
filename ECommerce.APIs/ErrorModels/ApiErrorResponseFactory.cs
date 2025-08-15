﻿
using ECommerce.Core.Common.Constants;
using Microsoft.Extensions.Localization;

namespace ECommerce.APIs.ErrorModels;

public class ApiErrorResponseFactory : IApiErrorResponseFactory
{
	private readonly IStringLocalizer<ApiErrorResponse> _localizer;
	private readonly IWebHostEnvironment _env;

	public ApiErrorResponseFactory(IStringLocalizer<ApiErrorResponse> localizer, IWebHostEnvironment env)
	{
		_localizer = localizer;
		_env = env;
	}

	public ApiErrorResponse CreateErrorResponse(int statusCode, string? message = null)
	{
		var defaultMessage = GenerateDefaultStatusMessage(statusCode);
		return new ApiErrorResponse(statusCode, message ?? defaultMessage);
	}

	public ApiExceptionErrorResponse CreateExceptionErrorResponse(Exception ex)
	{
		var defaultMessage = GenerateDefaultStatusMessage(StatusCodes.Status500InternalServerError);
		return new ApiExceptionErrorResponse(ex, _env, defaultMessage);
	}

	public ApiValidationErrorResponse CreateValidationErrorResponse(IEnumerable<string> errors)
	{
		var defaultMessage = GenerateDefaultStatusMessage(StatusCodes.Status400BadRequest);
		return new ApiValidationErrorResponse(errors, defaultMessage);
	}

	private string GenerateDefaultStatusMessage(int statusCode)
	{
		return statusCode switch
		{
			StatusCodes.Status400BadRequest =>
				_localizer[L.ApiErrors.BadRequest] ?? "The request was invalid or cannot be served.",

			StatusCodes.Status401Unauthorized =>
				_localizer[L.ApiErrors.Unauthorized] ?? "Authentication is required and has failed or has not yet been provided.",

			StatusCodes.Status403Forbidden =>
				_localizer[L.ApiErrors.Forbidden] ?? "You do not have permission to access this resource.",

			StatusCodes.Status404NotFound =>
				_localizer[L.ApiErrors.NotFound] ?? "The requested resource could not be found.",

			StatusCodes.Status500InternalServerError =>
				_localizer[L.ApiErrors.InternalServerError] ?? "An unexpected error occurred on the server.",

			_ => _localizer[L.ApiErrors.GenericError] ?? "An error occurred.",
		};
	}
}
