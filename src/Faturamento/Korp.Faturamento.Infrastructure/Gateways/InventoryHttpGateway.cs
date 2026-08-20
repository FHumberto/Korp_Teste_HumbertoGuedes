using System.Net;
using System.Net.Http.Json;
using Korp.Faturamento.Application.Contracts.Gateways;

namespace Korp.Faturamento.Infrastructure.Gateways;

public sealed class InventoryHttpGateway(HttpClient httpClient) : IInventoryGateway
{
    public async Task<IReadOnlyCollection<InventoryProduct>> GetProductsByIdsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                "api/v1/products/lookup",
                new InventoryLookupRequest(productIds),
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return [];

            if (!response.IsSuccessStatusCode)
                throw new InventoryUnavailableException($"O Estoque respondeu com o status {(int)response.StatusCode}.");

            IReadOnlyCollection<InventoryProduct>? products =
                await response.Content.ReadFromJsonAsync<IReadOnlyCollection<InventoryProduct>>(cancellationToken);

            return products ?? [];
        }
        catch (InventoryUnavailableException)
        {
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            throw new InventoryUnavailableException("Não foi possível consultar o serviço de Estoque.", exception);
        }
    }

    private sealed record InventoryLookupRequest(IReadOnlyCollection<Guid> ProductIds);
}
