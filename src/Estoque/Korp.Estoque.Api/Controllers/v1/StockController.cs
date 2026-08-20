using Asp.Versioning;
using Korp.Estoque.Application.Contracts.UseCases;
using Korp.Estoque.Application.Features.Stock.DebitStock;

namespace Korp.Estoque.Api.Controllers.v1;

[Tags("Stock")]
[ApiVersion("1")]
public sealed class StockController : BaseController
{
    [HttpPost("debits")]
    [ProducesResponseType<DebitStockResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DebitAsync([FromServices] IDebitStockUseCase useCase, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, [FromBody] DebitStockRequest? request, CancellationToken ct)
    {
        DebitStockCommand command = request is null ? new DebitStockCommand(idempotencyKey, Guid.Empty, [])
                                                    : new DebitStockCommand(idempotencyKey, request.InvoiceId, request.Items);

        return (await useCase.ExecuteAsync(command, ct)).Match(onSuccess: Ok, onFailure: Problem);
    }
}
