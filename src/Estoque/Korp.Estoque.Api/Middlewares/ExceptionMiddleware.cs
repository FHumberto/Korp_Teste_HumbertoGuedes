using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Korp.Estoque.Api.Middlewares;

public sealed class ExceptionMiddleware(IProblemDetailsService problemDetailsService, ILogger<ExceptionMiddleware> logger) : IExceptionHandler
{
    #region [ CONSTANTES ]

    private const int ClientClosedRequestStatusCode = 499;

    #endregion

    #region [ HANDLER ]

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
            return false;

        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            httpContext.Response.StatusCode = ClientClosedRequestStatusCode;
            return true;
        }

        ProblemDetails problemDetails = CreateProblemDetails(httpContext, exception);

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    #endregion

    #region [ AUXILIARES ]

    private ProblemDetails CreateProblemDetails(HttpContext httpContext, Exception exception)
    {
        ProblemDetails problemDetails;

        switch (exception)
        {
            case NotImplementedException:
                problemDetails = new ProblemDetails
                {
                    Type = "https://www.rfc-editor.org/rfc/rfc9110#section-15.6.2",
                    Status = StatusCodes.Status501NotImplemented,
                    Title = "Este recurso ainda não está disponível."
                };

                LogException(httpContext, exception, LogLevel.Warning);
                break;

            case OperationCanceledException:
                problemDetails = new ProblemDetails
                {
                    Type = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.9",
                    Status = StatusCodes.Status408RequestTimeout,
                    Title = "A requisição foi cancelada antes de ser concluída."
                };

                LogException(httpContext, exception, LogLevel.Information);
                break;

            default:
                problemDetails = new ProblemDetails
                {
                    Type = "https://www.rfc-editor.org/rfc/rfc9110#section-15.6.1",
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Ocorreu um erro interno ao processar a requisição."
                };

                LogException(httpContext, exception, LogLevel.Error);
                break;
        }

        problemDetails.Instance = httpContext.Request.Path;
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        return problemDetails;
    }

    private void LogException(HttpContext httpContext, Exception exception, LogLevel logLevel)
    {
        if (!logger.IsEnabled(logLevel))
            return;

        logger.Log
        (
            logLevel,
            exception,
            "Falha ao processar {Method} {Path}. TraceId={TraceId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.TraceIdentifier
        );
    }

    #endregion
}
