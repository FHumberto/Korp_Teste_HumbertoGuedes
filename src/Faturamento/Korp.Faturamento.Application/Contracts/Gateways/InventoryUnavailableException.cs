namespace Korp.Faturamento.Application.Contracts.Gateways;

public sealed class InventoryUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
