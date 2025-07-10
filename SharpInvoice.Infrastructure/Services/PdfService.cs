using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Interfaces.Services;

namespace SharpInvoice.Infrastructure.Services;

public class PdfService : IPdfService
{
    public async Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice)
    {
        // QuestPDF is synchronous, but we'll keep the async signature for consistency
        return await Task.FromResult(GenerateInvoicePdf(invoice));
    }

    public async Task<Stream> GenerateInvoicePdfStreamAsync(Invoice invoice)
    {
        var bytes = await GenerateInvoicePdfAsync(invoice);
        return new MemoryStream(bytes);
    }

    private static byte[] GenerateInvoicePdf(Invoice invoice)
    {
        // Set QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
        
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.DefaultTextStyle(x => x.FontSize(10));
                
                page.Header().Element(header =>
                {
                    header.Row(row =>
                    {
                        // Business logo and info
                        row.RelativeItem().Column(column =>
                        {
                            if (!string.IsNullOrEmpty(invoice.Business.LogoUrl))
                            {
                                column.Item().Height(50).Image(invoice.Business.LogoUrl);
                            }
                            
                            column.Item().Text(invoice.Business.Name).FontSize(20).Bold();
                            column.Item().Text(invoice.Business.GetFormattedAddress());
                            
                            if (!string.IsNullOrEmpty(invoice.Business.Email))
                                column.Item().Text($"Email: {invoice.Business.Email}");
                                
                            if (!string.IsNullOrEmpty(invoice.Business.PhoneNumber))
                                column.Item().Text($"Phone: {invoice.Business.PhoneNumber}");
                                
                            if (!string.IsNullOrEmpty(invoice.Business.Website))
                                column.Item().Text($"Web: {invoice.Business.Website}");
                        });
                        
                        // Invoice details
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().AlignRight().Text("INVOICE").FontSize(20).Bold();
                            column.Item().AlignRight().Text($"Invoice #: {invoice.InvoiceNumber}");
                            column.Item().AlignRight().Text($"Issue Date: {invoice.IssueDate:d}");
                            column.Item().AlignRight().Text($"Due Date: {invoice.DueDate:d}");
                            column.Item().AlignRight().Text($"Status: {invoice.Status}");
                        });
                    });
                });
                
                // Client information
                page.Content().Element(content =>
                {
                    content.PaddingVertical(10);
                    
                    // Client details
                    content.Column(column =>
                    {
                        column.Item().Text("Bill To:").Bold();
                        column.Item().Text(invoice.Client.Name);
                        
                        if (!string.IsNullOrEmpty(invoice.Client.Address))
                            column.Item().Text(invoice.Client.Address);
                            
                        if (!string.IsNullOrEmpty(invoice.Client.Email))
                            column.Item().Text($"Email: {invoice.Client.Email}");
                            
                        if (!string.IsNullOrEmpty(invoice.Client.Phone))
                            column.Item().Text($"Phone: {invoice.Client.Phone}");
                            
                        column.Item().PaddingVertical(10);
                        
                        // Invoice items table
                        column.Item().Table(table =>
                        {
                            // Define columns
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            
                            // Add header row
                            table.Header(header =>
                            {
                                header.Cell().Background("#EEEEEE").Padding(5).Text("Description").Bold();
                                header.Cell().Background("#EEEEEE").Padding(5).Text("Quantity").Bold();
                                header.Cell().Background("#EEEEEE").Padding(5).Text("Unit Price").Bold();
                                header.Cell().Background("#EEEEEE").Padding(5).Text("Total").Bold();
                            });
                            
                            // Add data rows
                            foreach (var item in invoice.Items)
                            {
                                table.Cell().Padding(5).Text(item.Description);
                                table.Cell().Padding(5).Text(item.Quantity.ToString("0.##"));
                                table.Cell().Padding(5).Text($"{item.UnitPrice.ToString("C2")} {invoice.Currency}");
                                table.Cell().Padding(5).Text($"{item.Total.ToString("C2")} {invoice.Currency}");
                            }
                        });
                        
                        // Summary
                        column.Item().PaddingTop(10).AlignRight().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            
                            table.Cell().Padding(5).Text("Subtotal:").Bold();
                            table.Cell().Padding(5).Text($"{invoice.SubTotal.ToString("C2")} {invoice.Currency}");
                            
                            table.Cell().Padding(5).Text("Tax:").Bold();
                            table.Cell().Padding(5).Text($"{invoice.Tax.ToString("C2")} {invoice.Currency}");
                            
                            table.Cell().Padding(5).Text("Total:").Bold();
                            table.Cell().Padding(5).Text($"{invoice.Total.ToString("C2")} {invoice.Currency}").Bold();
                            
                            table.Cell().Padding(5).Text("Amount Paid:").Bold();
                            table.Cell().Padding(5).Text($"{invoice.AmountPaid.ToString("C2")} {invoice.Currency}");
                            
                            table.Cell().Padding(5).Text("Balance Due:").Bold();
                            table.Cell().Padding(5).Text($"{(invoice.Total - invoice.AmountPaid).ToString("C2")} {invoice.Currency}").Bold();
                        });
                    });
                });
                
                // Notes and terms
                page.Footer().Element(footer =>
                {
                    footer.Column(column =>
                    {
                        if (!string.IsNullOrEmpty(invoice.Notes))
                        {
                            column.Item().PaddingTop(10).Text("Notes:").Bold();
                            column.Item().Text(invoice.Notes);
                        }
                        
                        if (!string.IsNullOrEmpty(invoice.Terms))
                        {
                            column.Item().PaddingTop(10).Text("Terms and Conditions:").Bold();
                            column.Item().Text(invoice.Terms);
                        }
                        
                        column.Item().PaddingTop(10).AlignCenter().Text(text =>
                        {
                            text.Span("Thank you for your business!").Bold();
                        });
                    });
                });
            });
        }).GeneratePdf();
    }
} 