using Asp.Versioning;

namespace Korp.Estoque.Api.Controllers;

[Route("/")]
[Tags("A P I")]
[ApiVersionNeutral]
public sealed class DefaultController : BaseController
{
    [HttpGet]
    [EndpointSummary("Checagem")]
    [EndpointDescription("Verifica se a API está funcional.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IResult> Get() => TypedResults.Ok("Korp.Estoque.Api is running!");
}
