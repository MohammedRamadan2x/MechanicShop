using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Billing;

using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using QuestPDF.Fluent;

namespace MechanicShop.Infrastructure.Services;

public sealed class InvoicePdfGenerator : IInvoicePdfGenerator
{
    public byte[] Generate(Invoice invoice)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Header().Element(BuildHeader(invoice));
                page.Content().Element(BuildInvoiceContent(invoice));
                page.Footer().Element(BuildFooter());
            });
        })
        .GeneratePdf();
    }

    private Action<IContainer> BuildHeader(Invoice invoice) => header =>
    {
        header.Column(col =>
        {
            // Logo and Company Name Row
            col.Item().Row(row =>
            {
                row.RelativeItem(1).Element(container =>
                {
                    container.Row(logoRow =>
                    {
                        // Car repair icon placeholder
                        logoRow.ConstantItem(50)
                            .Height(50)
                            .Background(Colors.Red.Medium)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text("🔧")
                            .FontSize(24)
                            .FontColor(Colors.White);

                        // Company name with colorful letters
                        logoRow.RelativeItem()
                        .PaddingLeft(15)
                        .AlignMiddle()
                        .Text(text =>
                        {
                            text.Span("M")
                            .FontColor(Colors.Cyan.Lighten3)
                            .FontSize(24)
                            .Bold();

                            text.Span("e")
                            .FontColor(Colors.Orange.Medium)
                            .FontSize(24)
                            .Bold();

                            text.Span("c")
                            .FontColor(Colors.Yellow.Medium)
                            .FontSize(24)
                            .Bold();

                            text.Span("h")
                            .FontColor(Colors.Green.Medium)
                            .FontSize(24)
                            .Bold();

                            text.Span("a")
                            .FontColor(Colors.Blue.Lighten2)
                            .FontSize(24)
                            .Bold();

                            text.Span("n")
                            .FontColor(Colors.Red.Lighten2)
                            .FontSize(24)
                            .Bold();

                            text.Span("i")
                            .FontColor(Colors.Purple.Medium)
                            .FontSize(24)
                            .Bold();

                            text.Span("c")
                            .FontColor(Colors.Brown.Medium)
                            .FontSize(24)
                            .Bold();

                            text.Span(" ")
                            .FontSize(24);

                            text.Span("S")
                            .FontColor(Colors.Red.Darken1)
                            .FontSize(24)
                            .Bold();

                            text.Span("h")
                            .FontColor(Colors.Red.Medium)
                            .FontSize(24)
                            .Bold();

                            text.Span("o")
                            .FontColor(Colors.Green.Lighten1)
                            .FontSize(24)
                            .Bold();

                            text.Span("P")
                            .FontColor(Colors.Pink.Lighten2)
                            .FontSize(24)
                            .Bold();
                        });
                    });
                });

                // Invoice details
                row.RelativeItem(1).AlignRight().Column(detailsCol =>
                {
                    detailsCol.Item()
                        .Text($"INVOICE #{invoice.Id.ToString().Substring(0, 8)}")
                        .FontSize(28)
                        .Bold()
                        .FontColor(Colors.Grey.Darken3);

                    detailsCol.Item()
                        .PaddingTop(5)
                        .Text($"Date: {invoice.IssuedAtUtc:MMMM dd, yyyy}")
                        .FontSize(12)
                        .FontColor(Colors.Grey.Medium);

                    detailsCol.Item()
                        .Text($"Status: {invoice.Status}")
                        .FontSize(12)
                        .FontColor(GetStatusColor(invoice.Status.ToString()));
                });
            });

            // Separator line
            col.Item()
            .PaddingVertical(20)
            .LineHorizontal(2)
            .LineColor(Colors.Grey.Darken2);
        });
    };

    private Action<IContainer> BuildInvoiceContent(Invoice invoice) => content =>
    {
        content.Column(col =>
        {
            // Professional table with enhanced styling
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4); // Description
                    columns.RelativeColumn(1); // Qty
                    columns.RelativeColumn(2); // Unit Price
                    columns.RelativeColumn(2); // Line Total
                });

                // Enhanced header
                table.Header(header =>
                {
                    header.Cell()
                        .Background(Colors.Grey.Darken2)
                        .Padding(12)
                        .Text("DESCRIPTION")
                        .Bold()
                        .FontColor(Colors.White)
                        .FontSize(11);

                    header.Cell()
                        .Background(Colors.Grey.Darken2)
                        .Padding(12)
                        .AlignCenter()
                        .Text("QTY")
                        .Bold()
                        .FontColor(Colors.White)
                        .FontSize(11);

                    header.Cell()
                        .Background(Colors.Grey.Darken2)
                        .Padding(12)
                        .AlignCenter()
                        .Text("UNIT PRICE")
                        .Bold()
                        .FontColor(Colors.White)
                        .FontSize(11);

                    header.Cell()
                        .Background(Colors.Grey.Darken2)
                        .Padding(12)
                        .AlignRight()
                        .Text("LINE TOTAL")
                        .Bold()
                        .FontColor(Colors.White)
                        .FontSize(11);
                });

                // Enhanced rows with alternating colors
                var isEvenRow = false;

                foreach (var item in invoice.LineItems)
                {
                    var backgroundColor = isEvenRow ? 
                        Colors.Grey.Lighten4 : Colors.White;

                    table.Cell()
                        .Background(backgroundColor)
                        .Padding(12)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Text(item.Description)
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken3);

                    table.Cell()
                        .Background(backgroundColor)
                        .Padding(12)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .AlignCenter()
                        .Text(item.Quantity.ToString())
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken3);

                    table.Cell()
                        .Background(backgroundColor)
                        .Padding(12)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .AlignCenter()
                        .Text($"{item.UnitPrice:C}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken3);

                    table.Cell()
                        .Background(backgroundColor)
                        .Padding(12)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .AlignRight()
                        .Text($"{item.LineTotal:C}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken3)
                        .Bold();

                    isEvenRow = !isEvenRow;
                }
            });

            // Enhanced totals section
            col.Item().PaddingTop(30).Row(row =>
            {
                row.RelativeItem(2); // Empty space

                row.RelativeItem(1).Column(totalsCol =>
                {
                    totalsCol.Item()
                    .BorderTop(1)
                    .BorderColor(Colors.Grey.Medium)
                    .PaddingTop(10);

                    totalsCol.Item().PaddingVertical(5).Row(totalRow =>
                    {
                        totalRow.RelativeItem()
                        .Text("Subtotal:")
                        .FontSize(11)
                        .FontColor(Colors.Grey.Medium);

                        totalRow.RelativeItem()
                        .AlignRight()
                        .Text($"{invoice.Subtotal:C}")
                        .FontSize(11)
                        .FontColor(Colors.Grey.Darken3);
                    });

                    totalsCol.Item().PaddingVertical(5).Row(totalRow =>
                    {
                        totalRow.RelativeItem()
                        .Text("Tax:")
                        .FontSize(11)
                        .FontColor(Colors.Grey.Medium);

                        totalRow.RelativeItem()
                        .AlignRight()
                        .Text($"{invoice.TaxAmount:C}")
                        .FontSize(11)
                        .FontColor(Colors.Grey.Darken3);
                    });

                    if (invoice.DiscountAmount > 0)
                    {
                        totalsCol.Item().PaddingVertical(5).Row(totalRow =>
                        {
                            totalRow.RelativeItem()
                            .Text("Discount:")
                            .FontSize(11)
                            .FontColor(Colors.Red.Medium);

                            totalRow.RelativeItem()
                            .AlignRight()
                            .Text($"-{invoice.DiscountAmount:C}")
                            .FontSize(11)
                            .FontColor(Colors.Red.Medium);
                        });
                    }

                    totalsCol.Item()
                        .BorderTop(2)
                        .BorderColor(Colors.Grey.Darken3)
                        .PaddingTop(10)
                        .PaddingVertical(5)
                        .Background(Colors.Grey.Lighten3)
                        .Padding(10)
                        .Row(totalRow =>
                        {
                            totalRow.RelativeItem()
                            .Text("TOTAL:")
                            .FontSize(14)
                            .Bold()
                            .FontColor(Colors.Grey.Darken3);

                            totalRow.RelativeItem()
                            .AlignRight()
                            .Text($"{invoice.Total:C}")
                            .FontSize(16)
                            .Bold()
                            .FontColor(Colors.Green.Medium);
                        });
                });
            });
        });
    };

    private Action<IContainer> BuildFooter() => footer =>
    {
        footer.Row(row =>
        {
            row.RelativeItem()
                .AlignLeft()
                .Text("Drive safe. See you next time!")
                .FontSize(10)
                .FontColor(Colors.Grey.Medium)
                .Italic();

            row.RelativeItem()
                .AlignRight()
                .Text(text =>
                {
                    text.Span("Generated on ")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);

                    text.Span($"{DateTime.UtcNow:MMMM dd, yyyy 'at' HH:mm} UTC")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium)
                        .SemiBold();
                });
        });
    };

    private string GetStatusColor(string status)
    {
        return status.ToLower() switch
        {
            "paid" => Colors.Green.Medium,
            "scheduled" => Colors.Orange.Medium,
            "overdue" => Colors.Red.Medium,
            "cancelled" => Colors.Grey.Medium,
            _ => Colors.Grey.Medium
        };
    }
}