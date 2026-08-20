using Korp.Faturamento.Domain.Entities;

namespace Korp.Faturamento.Application.Contracts.Documents;

public interface IInvoiceDocumentGenerator
{
    byte[] Generate(Invoice invoice);
}
