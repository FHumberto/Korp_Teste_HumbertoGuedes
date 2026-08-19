using Korp.Estoque.Domain.Abstractions.Types;
using Microsoft.AspNetCore.Mvc;

namespace Korp.Estoque.Api.Controllers;

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
            _ => StatusCodes.Status500InternalServerError
        };

        if (error.ValidationDetails is not null && error.ErrorType == ErrorType.Validation)
        {
            ProblemDetails problemDetails = new()
            {
                Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
                Title = error.Description,
                Detail = error.Code,
                Status = statusCode
            };

            problemDetails.Extensions["errors"] = error.ValidationDetails;
            problemDetails.Extensions["traceId"] = HttpContext.TraceIdentifier;

            return StatusCode(statusCode, problemDetails);
        }

        return Problem
        (
            statusCode: statusCode,
            detail: error.Code,
            title: error.Description
        );
    }

    #endregion
}
