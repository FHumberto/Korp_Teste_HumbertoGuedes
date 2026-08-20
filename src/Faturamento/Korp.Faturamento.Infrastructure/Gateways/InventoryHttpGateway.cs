using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

    public async Task<DebitStockResult> DebitAsync(
        DebitStockCommand command,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/stock/debits")
            {
                Content = JsonContent.Create(command)
            };
            request.Headers.Add("Idempotency-Key", idempotencyKey);

            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
                return DebitStockResult.Succeeded;

            string? errorCode = await ReadErrorCodeAsync(response, cancellationToken);

            return (response.StatusCode, errorCode) switch
            {
                (HttpStatusCode.NotFound, _) => new DebitStockResult(DebitStockStatus.ProductNotFound),
                (HttpStatusCode.Conflict, "INSUFFICIENT_STOCK") => new DebitStockResult(DebitStockStatus.InsufficientStock),
                (HttpStatusCode.Conflict, "IDEMPOTENCY_CONFLICT") => new DebitStockResult(DebitStockStatus.IdempotencyConflict),
                _ => throw new InventoryUnavailableException($"O Estoque respondeu com o status {(int)response.StatusCode}.")
            };
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
            throw new InventoryUnavailableException("Não foi possível solicitar a baixa ao serviço de Estoque.", exception);
        }
    }

    private static async Task<string?> ReadErrorCodeAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            using JsonDocument document = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);

            if (document.RootElement.TryGetProperty("code", out JsonElement code))
                return code.GetString();

            return document.RootElement.TryGetProperty("detail", out JsonElement detail)
                ? detail.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record InventoryLookupRequest(IReadOnlyCollection<Guid> ProductIds);
}
