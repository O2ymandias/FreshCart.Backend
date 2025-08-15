namespace ECommerce.APIs.ErrorModels;

public class ApiValidationErrorResponse : ApiErrorResponse
{
	public ApiValidationErrorResponse(IEnumerable<string> errors, string message)
		: base(StatusCodes.Status400BadRequest, message)
	{
		Errors = [.. errors];
	}
	public List<string> Errors { get; set; }

}
