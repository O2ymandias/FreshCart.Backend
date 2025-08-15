
using ECommerce.APIs.ErrorModels;

namespace ECommerce.APIs.Middlewares;

public class ExceptionMiddleware : IMiddleware
{
	private readonly ILogger<ExceptionMiddleware> _logger;
	private readonly IApiErrorResponseFactory _errorFactory;

	public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger, IApiErrorResponseFactory errorFactory)
	{
		_logger = logger;
		_errorFactory = errorFactory;
	}

	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		try
		{
			await next.Invoke(context);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Error: {Message} occurred at {Endpoint}",
				ex.Message,
				context.GetEndpoint()?.DisplayName ?? string.Empty
			);

			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			var error = _errorFactory.CreateExceptionErrorResponse(ex);
			await context.Response.WriteAsJsonAsync(error);
		}
	}
}
