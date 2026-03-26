using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using C__GestionDepenses.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace C__GestionDepenses.Services.Pdf
{
    public class MonthlyActivityPdfDocument : IDocument
    {
        private readonly string _title;
        private readonly DateTime _monthStart;
        private readonly IReadOnlyList<Depense> _depenses;
        private readonly IReadOnlyList<Revenu> _revenus;

        public MonthlyActivityPdfDocument(
            string title,
            DateTime monthStart,
            IReadOnlyList<Depense> depenses,
            IReadOnlyList<Revenu> revenus)
        {
            _title = title;
            _monthStart = monthStart;
            _depenses = depenses;
            _revenus = revenus;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            var culture = CultureInfo.CurrentCulture;
            var monthLabel = _monthStart.ToString("Y", culture);

            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text(_title).FontSize(18).SemiBold();
                    col.Item().Text($"Monthly activity: {monthLabel}").FontSize(12).FontColor(Colors.Grey.Darken2);
                });

                page.Content().Column(col =>
                {
                    var totalRevenus = _revenus.Sum(r => r.Montant);
                    var totalDepenses = _depenses.Sum(d => d.Montant);
                    var balance = totalRevenus - totalDepenses;

                    col.Spacing(15);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Element(e => SummaryCard(e, "Total revenus", totalRevenus.ToString("C", culture), Colors.Green.Lighten4));
                        row.RelativeItem().Element(e => SummaryCard(e, "Total dépenses", totalDepenses.ToString("C", culture), Colors.Red.Lighten4));
                        row.RelativeItem().Element(e => SummaryCard(e, "Balance", balance.ToString("C", culture), balance >= 0 ? Colors.Green.Lighten5 : Colors.Red.Lighten5));
                    });

                    col.Item().Text("Revenus").FontSize(14).SemiBold();
                    col.Item().Element(e => TransactionsTable(e,
                        _revenus.Select(r => new TxRow(r.Date, r.Categorie?.Nom, r.Description, r.Montant)).ToList(),
                        culture));

                    col.Item().Text("Dépenses").FontSize(14).SemiBold();
                    col.Item().Element(e => TransactionsTable(e,
                        _depenses.Select(d => new TxRow(d.Date, d.Categorie?.Nom, d.Description, d.Montant)).ToList(),
                        culture));
                });

                page.Footer().AlignCenter()
                    .Text($"Generated on {DateTime.Now.ToString("g", culture)}")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);
            });
        }

        private static void SummaryCard(IContainer container, string label, string value, string background)
        {
            container
                .Padding(10)
                .Background(background)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Column(col =>
                {
                    col.Item().Text(label).FontSize(10).FontColor(Colors.Grey.Darken2);
                    col.Item().Text(value).FontSize(14).SemiBold();
                });
        }

        private record TxRow(DateTime Date, string? Category, string? Description, decimal Amount);

        private static void TransactionsTable(IContainer container, List<TxRow> rows, CultureInfo culture)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(90);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeaderStyle).Text("Date");
                    header.Cell().Element(CellHeaderStyle).Text("Category");
                    header.Cell().Element(CellHeaderStyle).Text("Description");
                    header.Cell().Element(CellHeaderStyle).AlignRight().Text("Amount");
                });

                foreach (var r in rows.OrderByDescending(r => r.Date))
                {
                    table.Cell().Element(CellStyle).Text(r.Date.ToString("d", culture));
                    table.Cell().Element(CellStyle).Text(r.Category ?? string.Empty);
                    table.Cell().Element(CellStyle).Text(r.Description ?? string.Empty);
                    table.Cell().Element(CellStyle).AlignRight().Text(r.Amount.ToString("C", culture));
                }

                static IContainer CellHeaderStyle(IContainer c) => c
                    .DefaultTextStyle(x => x.SemiBold())
                    .PaddingVertical(6)
                    .PaddingHorizontal(4)
                    .Background(Colors.Grey.Lighten3)
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten1);

                static IContainer CellStyle(IContainer c) => c
                    .PaddingVertical(4)
                    .PaddingHorizontal(4)
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten3);
            });
        }
    }
}
