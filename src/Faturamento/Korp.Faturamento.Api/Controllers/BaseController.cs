using Korp.Faturamento.Domain.Abstractions.Types;

namespace Korp.Faturamento.Api.Controllers;

[ApiController]
[Route("api/v{v:apiVersion}/[controller]")]
public class BaseController : ControllerBase
{
    #region [ HTTP ]

    protected IActionResult Problem(Error error)
    {
        int statusCode = error.ErrorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.AccessUnauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.AccessForbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.ServiceUnavailable => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };

        ProblemDetails problemDetails = new()
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = error.Description,
            Detail = error.Description,
            Status = statusCode,
            Instance = HttpContext.Request.Path
        };

        problemDetails.Extensions["code"] = error.Code;
        problemDetails.Extensions["traceId"] = HttpContext.TraceIdentifier;

        if (error.ValidationDetails is not null && error.ErrorType == ErrorType.Validation)
            problemDetails.Extensions["errors"] = error.ValidationDetails;

        return StatusCode(statusCode, problemDetails);
    }

    #endregion
}
