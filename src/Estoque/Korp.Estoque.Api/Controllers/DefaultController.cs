using Microsoft.AspNetCore.Mvc;

namespace Korp.Estoque.Api.Controllers;

[ApiController]
[Route("/")]
[Tags("A P I")]
public sealed class DefaultController : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Checagem")]
    [EndpointDescription("Verifica se a API está funcional.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IResult> Get() => TypedResults.Ok("Korp.Estoque.Api is running!");
}
