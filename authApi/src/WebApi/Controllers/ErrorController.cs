using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;

namespace WebApi.Controllers;

[ApiController]
[Route("error")]
public class ErrorController : ControllerBase
{
	[HttpGet]
	[HttpPost]
	[HttpPut]
	[HttpDelete]
	public IActionResult HandleError()
	{
		var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
		var exception = exceptionFeature?.Error;

		if (exception is null)
		{
			return new JsonResult(new { message = "An unexpected error occurred" }) { StatusCode = 500 };
		}

		var message = exception.Message;
		var statusCode = 500;

		if (exception is UnauthorizedAccessException)
		{
			statusCode = 401;
		}
		else if (exception is ArgumentException || exception is ArgumentNullException)
		{
			statusCode = 400;
		}
		else if (exception is InvalidOperationException)
		{
			statusCode = message.Contains("not found", StringComparison.OrdinalIgnoreCase) ? 404 : 400;
		}
		else if (exception is KeyNotFoundException)
		{
			statusCode = 404;
		}

		return new JsonResult(new { message }) { StatusCode = statusCode };
	}
}
