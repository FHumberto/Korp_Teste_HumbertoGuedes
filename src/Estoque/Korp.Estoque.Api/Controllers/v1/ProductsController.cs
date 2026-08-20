using Asp.Versioning;
using Korp.Estoque.Application.Contracts.UseCases;
using Korp.Estoque.Application.Features.Product.CreateProduct;
using Korp.Estoque.Application.Features.Product.GetProduct;
using Korp.Estoque.Application.Features.Product.GetProductsByIds;
using Korp.Estoque.Application.Features.Product.ListProducts;
using Korp.Estoque.Domain.Abstractions.Types;

namespace Korp.Estoque.Api.Controllers.v1;

[Tags("Products")]
[ApiVersion("1")]
public sealed class ProductsController : BaseController
{
    #region [ LEITURA ]

    [HttpGet]
    [ProducesResponseType<Paged<ListProductsResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListProductsAsync([FromServices] IListProductsUseCase useCase, [FromQuery] ListProductsRequest request, CancellationToken ct)
    {
        return (await useCase.ExecuteAsync(request, ct)).Match(onSuccess: Ok, onFailure: Problem);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById([FromServices] IGetProductUseCase useCase, Guid id, CancellationToken ct)
    {
        return (await useCase.ExecuteAsync(id, ct)).Match(onSuccess: Ok, onFailure: Problem);
    }

    #endregion

    #region [ ESCRITA ]

    [HttpPost]
    [ProducesResponseType<CreateProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateProductAsync([FromServices] ICreateProductUseCase useCase, [FromBody] CreateProductRequest request, CancellationToken ct)
    {
        return (await useCase.ExecuteAsync(request, ct)).Match
        (
            onSuccess: dto => CreatedAtAction(actionName: nameof(GetProductById), routeValues: new { id = dto.Id }, value: dto),
            onFailure: Problem
        );
    }

    [HttpPost("lookup")]
    [ProducesResponseType<IReadOnlyCollection<GetProductsByIdsResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductsByIdsAsync([FromServices] IGetProductsByIdsUseCase useCase, [FromBody] GetProductsByIdsRequest request, CancellationToken ct)
    {
        return (await useCase.ExecuteAsync(request, ct)).Match(onSuccess: Ok, onFailure: Problem);
    }

    #endregion
}
