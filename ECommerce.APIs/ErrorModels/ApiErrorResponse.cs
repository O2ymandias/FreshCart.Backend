namespace ECommerce.APIs.ErrorModels;

public class ApiErrorResponse
{
	public ApiErrorResponse(int statusCode, string message)
	{
		StatusCode = statusCode;
		Message = message;
	}

	public int StatusCode { get; set; }
	public string Message { get; set; }
}
