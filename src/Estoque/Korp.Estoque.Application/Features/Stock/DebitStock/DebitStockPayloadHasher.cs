using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Korp.Estoque.Application.Features.Stock.DebitStock;

internal static class DebitStockPayloadHasher
{
    public static string Compute(Guid invoiceId, IReadOnlyCollection<DebitStockItemRequest> items)
    {
        StringBuilder canonicalPayload = new();
        canonicalPayload.Append(invoiceId.ToString("N"));

        foreach (DebitStockItemRequest item in items.OrderBy(item => item.ProductId))
        {
            canonicalPayload
                .Append('|')
                .Append(item.ProductId.ToString("N"))
                .Append(':')
                .Append(item.Quantity.ToString(CultureInfo.InvariantCulture));
        }

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalPayload.ToString()));
        return Convert.ToHexStringLower(hash);
    }
}
