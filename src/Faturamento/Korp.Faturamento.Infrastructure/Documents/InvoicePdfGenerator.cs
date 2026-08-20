using System.Globalization;
using Korp.Faturamento.Application.Contracts.Documents;
using Korp.Faturamento.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Korp.Faturamento.Infrastructure.Documents;

public sealed class InvoicePdfGenerator : IInvoiceDocumentGenerator
{
    private static readonly CultureInfo PortugueseBrazil = CultureInfo.GetCultureInfo("pt-BR");

    public byte[] Generate(Invoice invoice)
    {
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(32);
            page.DefaultTextStyle(text => text.FontFamily(Fonts.Arial).FontSize(10).FontColor(Colors.Grey.Darken3));

            page.Header().Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Korp - Sistema de Emissão de Notas").Bold().FontSize(16).FontColor(Colors.Blue.Darken3);
                    column.Item().PaddingTop(4).Text("Documento simplificado de saída").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
                row.ConstantItem(190).Column(column =>
                {
                    column.Item().AlignRight().Text($"NOTA Nº {invoice.Number:D6}").Bold().FontSize(15);
                    column.Item().PaddingTop(4).AlignRight().Text("FECHADA").Bold().FontSize(9).FontColor(Colors.Green.Darken2);
                });
            });

            page.Content().PaddingVertical(24).Column(column =>
            {
                column.Spacing(18);
                column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12).Row(row =>
                {
                    row.RelativeItem().Text(text => { text.Span("Emissão\n").SemiBold(); text.Span(FormatDate(invoice.CreatedAt)); });
                    row.RelativeItem().Text(text => { text.Span("Fechamento\n").SemiBold(); text.Span(FormatDate(invoice.ClosedAt!.Value)); });
                });
                column.Item().Text("Itens da nota").Bold().FontSize(13);
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns => { columns.ConstantColumn(110); columns.RelativeColumn(); columns.ConstantColumn(90); });
                    table.Header(header =>
                    {
                        HeaderCell(header.Cell()).Text("Código");
                        HeaderCell(header.Cell()).Text("Descrição");
                        HeaderCell(header.Cell()).AlignRight().Text("Quantidade");
                    });
                    foreach (InvoiceItem item in invoice.Items)
                    {
                        BodyCell(table.Cell()).Text(item.ProductCode);
                        BodyCell(table.Cell()).Text(item.ProductDescription);
                        BodyCell(table.Cell()).AlignRight().Text(item.Quantity.ToString(PortugueseBrazil));
                    }
                });
                int totalUnits = invoice.Items.Sum(item => item.Quantity);
                column.Item().AlignRight().Text($"Produtos distintos: {invoice.Items.Count}   |   Total de unidades: {totalUnits}").Bold();
            });

            page.Footer().Column(column =>
            {
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                column.Item().PaddingTop(8).AlignCenter().Text("Documento demonstrativo sem validade fiscal. Não corresponde a NF-e ou DANFE oficial.").FontSize(8).FontColor(Colors.Grey.Darken1);
                column.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text($"Identificador: {invoice.Id}").FontSize(7);
                    row.RelativeItem().AlignRight().Text(text => { text.DefaultTextStyle(style => style.FontSize(7)); text.Span("Página "); text.CurrentPageNumber(); text.Span(" de "); text.TotalPages(); });
                });
            });
        })).WithMetadata(new DocumentMetadata
        {
            Title = $"Nota {invoice.Number:D6}", Author = "Korp - Sistema de Emissão de Notas",
            Subject = "Documento simplificado de saída", Creator = "Korp.Faturamento"
        }).GeneratePdf();
    }

    private static string FormatDate(DateTimeOffset date) => date.ToString("dd/MM/yyyy HH:mm 'UTC'zzz", PortugueseBrazil);
    private static IContainer HeaderCell(IContainer container) => container.Background(Colors.Blue.Darken3).PaddingVertical(8).PaddingHorizontal(7).DefaultTextStyle(text => text.Bold().FontColor(Colors.White));
    private static IContainer BodyCell(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(8).PaddingHorizontal(7);
}
