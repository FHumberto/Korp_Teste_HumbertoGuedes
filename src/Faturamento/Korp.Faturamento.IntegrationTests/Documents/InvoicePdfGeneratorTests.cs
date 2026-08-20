using Korp.Faturamento.Domain.Entities;
using Korp.Faturamento.Infrastructure.Documents;
using QuestPDF.Infrastructure;

namespace Korp.Faturamento.IntegrationTests.Documents;

public sealed class InvoicePdfGeneratorTests
{
    [Fact]
    public void Generate_WhenInvoiceIsClosed_ShouldCreatePdfWithContent()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        Invoice invoice = Invoice.Create(Guid.Parse("26ed264d-d430-42ce-b556-8a616ffedab1"), 42, new DateTimeOffset(2026, 8, 20, 10, 0, 0, TimeSpan.Zero));
        invoice.AddItem(Guid.NewGuid(), "PROD-001", "Produto para teste de impressão", 2);
        invoice.Close(new DateTimeOffset(2026, 8, 20, 10, 5, 0, TimeSpan.Zero));

        byte[] content = new InvoicePdfGenerator().Generate(invoice);

        content.Length.ShouldBeGreaterThan(1_000);
        content.Take(5).ToArray().ShouldBe("%PDF-"u8.ToArray());
    }
}
