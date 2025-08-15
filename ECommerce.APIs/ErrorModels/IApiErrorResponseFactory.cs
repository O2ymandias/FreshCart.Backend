namespace ECommerce.APIs.ErrorModels;

public interface IApiErrorResponseFactory
{
	ApiErrorResponse CreateErrorResponse(int statusCode, string? message = null);
	ApiExceptionErrorResponse CreateExceptionErrorResponse(Exception ex);
	ApiValidationErrorResponse CreateValidationErrorResponse(IEnumerable<string> errors);
}
