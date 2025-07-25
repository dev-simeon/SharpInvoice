using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SharpInvoice.Core.Entities;
using SharpInvoice.Core.Interfaces.Services;

namespace SharpInvoice.Infrastructure.Services;

public class PdfService : IPdfService
{
    public byte[] GenerateInvoicePdf(Invoice invoice)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header()
                    .Text($"Invoice #{invoice.InvoiceNumber}")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .Column(column =>
                    {
                        column.Spacing(20);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text(invoice.Business.Name).Bold();
                                col.Item().Text(invoice.Business.Address);
                                col.Item().Text(invoice.Business.Email);
                            });

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Bill To:").Bold();
                                col.Item().Text(invoice.Client.Name);
                                col.Item().Text(invoice.Client.Address);
                                col.Item().Text(invoice.Client.Email);
                            });
                        });

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Description");
                                header.Cell().AlignRight().Text("Quantity");
                                header.Cell().AlignRight().Text("Unit Price");
                                header.Cell().AlignRight().Text("Total");
                            });

                            foreach (var item in invoice.Items)
                            {
                                table.Cell().Text(item.Description);
                                table.Cell().AlignRight().Text(item.Quantity.ToString("N2"));
                                table.Cell().AlignRight().Text(item.UnitPrice.ToString("C"));
                                table.Cell().AlignRight().Text(item.Total.ToString("C"));
                            }

                            table.Cell().ColumnSpan(4).AlignRight().Text($"Subtotal: {invoice.SubTotal:C}");
                            table.Cell().ColumnSpan(4).AlignRight().Text($"Tax: {invoice.Tax:C}");
                            table.Cell().ColumnSpan(4).AlignRight().Text($"Total: {invoice.Total:C}").Bold();
                        });

                        if (!string.IsNullOrWhiteSpace(invoice.Notes))
                            column.Item().Text($"Notes: {invoice.Notes}");
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
            });
        }).GeneratePdf();
    }
} 