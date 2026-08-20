using Asp.Versioning;
using Korp.Faturamento.Application.Contracts.UseCases;
using Korp.Faturamento.Application.Features.Invoice.CloseInvoice;
using Korp.Faturamento.Application.Features.Invoice.CreateInvoice;
using Korp.Faturamento.Application.Features.Invoice.GetInvoice;
using Korp.Faturamento.Application.Features.Invoice.GetInvoiceDocument;
using Korp.Faturamento.Application.Features.Invoice.ListInvoices;

namespace Korp.Faturamento.Api.Controllers.v1;

[Tags("Invoices")]
[ApiVersion("1")]
[ControllerName("invoices")]
public sealed class InvoicesController : BaseController
{
    #region [ LEITURA ]

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<ListInvoicesResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListInvoicesAsync([FromServices] IListInvoicesUseCase useCase, [FromQuery] ListInvoicesRequest request, CancellationToken cancellationToken)
    {
        return (await useCase.ExecuteAsync(request, cancellationToken)).Match(onSuccess: Ok, onFailure: Problem);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetInvoiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceByIdAsync([FromServices] IGetInvoiceUseCase useCase, Guid id, CancellationToken cancellationToken)
    {
        return (await useCase.ExecuteAsync(id, cancellationToken)).Match(onSuccess: Ok, onFailure: Problem);
    }

    [HttpGet("{id:guid}/document.pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetInvoiceDocumentAsync([FromServices] IGetInvoiceDocumentUseCase useCase, Guid id, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, cancellationToken);
        return result.Match<GetInvoiceDocumentResponse, IActionResult>(
            onSuccess: response =>
            {
                Response.Headers.ContentDisposition = $"inline; filename=\"{response.FileName}\"";
                Response.Headers.CacheControl = "private, no-store";
                return File(response.Content, "application/pdf");
            },
            onFailure: Problem);
    }

    #endregion

    #region [ ESCRITA ]

    [HttpPost]
    [ProducesResponseType<CreateInvoiceResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateInvoiceAsync([FromServices] ICreateInvoiceUseCase useCase, [FromBody] CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        return (await useCase.ExecuteAsync(request, cancellationToken)).Match(
            onSuccess: response => Created($"/api/v1/invoices/{response.Id}", response),
            onFailure: Problem);
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType<CloseInvoiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CloseInvoiceAsync([FromServices] ICloseInvoiceUseCase useCase, Guid id, CancellationToken cancellationToken)
    {
        return (await useCase.ExecuteAsync(id, cancellationToken)).Match(onSuccess: Ok, onFailure: Problem);
    }

    #endregion
}
