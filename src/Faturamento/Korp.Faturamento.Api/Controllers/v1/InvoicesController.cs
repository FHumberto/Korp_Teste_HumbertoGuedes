using Asp.Versioning;
using Korp.Faturamento.Application.Abstractions.Wrappers;
using Korp.Faturamento.Application.Contracts.UseCases;
using Korp.Faturamento.Application.Features.Invoice.CreateInvoice;
using Microsoft.AspNetCore.Mvc;

namespace Korp.Faturamento.Api.Controllers.v1;

[Tags("Invoices")]
[ApiVersion("1")]
public sealed class InvoicesController : BaseController
{
    [HttpPost]
    [ProducesResponseType<CreateInvoiceResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateInvoiceAsync(
        [FromServices] ICreateInvoiceUseCase useCase,
        [FromBody] CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        return (await useCase.ExecuteAsync(request, cancellationToken)).Match(
            onSuccess: response => Created($"/api/v1/invoices/{response.Id}", response),
            onFailure: Problem);
    }
}
