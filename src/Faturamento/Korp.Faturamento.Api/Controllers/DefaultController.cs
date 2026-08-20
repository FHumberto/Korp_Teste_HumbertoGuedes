using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Korp.Faturamento.Api.Controllers;

[Route("/")]
[Tags("A P I")]
[ApiVersionNeutral]
public sealed class DefaultController : BaseController
{
    [HttpGet]
    [EndpointSummary("Checagem")]
    [EndpointDescription("Verifica se a API está funcional.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IResult> Get() => TypedResults.Ok("Korp.Faturamento.Api is running!");
}
