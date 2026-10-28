using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Nado_Graphics_MS_Backend.Services
{
    // Holds the logic for generating the PDF that matches Nado Graphics current quote layout
    public class QuotePDFService
    {
        private readonly IQuoteRepository _quotes;
        private readonly IRepository<Client> _clients;
        private readonly IProductRepository _products;
        private readonly CompanyInfoOptions _companyInfo;
        private readonly IWebHostEnvironment _environment;

        public QuotePDFService(
            IQuoteRepository quotes,
            IRepository<Client> clients,
            IProductRepository products,
            IOptions<CompanyInfoOptions> companyInfo,
            IWebHostEnvironment environment)
        {
            _quotes = quotes;
            _clients = clients;
            _products = products;
            _companyInfo = companyInfo.Value;
            _environment = environment;
        }
        //----------------------------------------------------------------------------------
        //salesRepName comes from the logged-in user who clicks to download.
        public async Task<byte[]?> GenerateQuotePdfAsync(int quoteId, string salesRepName)
        {
            var quote = await _quotes.GetWithItemsAsync(quoteId);

            if (quote == null)
            {
                return null;
            }

            var client = await _clients.GetByIdAsync(quote.ClientID);

            if (client == null)
            {
                return null;
            }

            var productIds = quote.QuoteItems.Select(i => i.ProductID).Distinct().ToList();
            var products = await _products.GetNamesByIdsAsync(productIds);

            var logoPath = Path.Combine(_environment.ContentRootPath, "Assets", "nado-logo.png");
            var hasLogo = File.Exists(logoPath);

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    //QUOTE Header and Nado Graphics Company logo
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Text("QUOTE").FontSize(28).Bold();

                        if (hasLogo)
                        {
                            row.ConstantItem(160).Image(logoPath);
                        }
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(15);

                        //Client info on the left / Date, Expiry, QuoteNumber Staff member who created he quote on the right
                        col.Item().Row(row =>
                        {
                            row.RelativeItem(2).Column(c =>
                            {
                                var clientFullName = $"{client.FirstName} {client.LastName}";

                                c.Item().Text(clientFullName).Bold();
                                c.Item().Text($"Attention: {clientFullName}");

                                if (!string.IsNullOrWhiteSpace(client.OrganisationName))
                                {
                                    c.Item().Text(client.OrganisationName);
                                }

                                //Address displayed below the client's name and organisation.
                                if (!string.IsNullOrWhiteSpace(client.StreetAddress))
                                {
                                    c.Item().Text(client.StreetAddress);
                                }

                                var cityLine = string.Join(" ", new[] { client.City, client.PostalCode }
                                    .Where(s => !string.IsNullOrWhiteSpace(s)));
                                if (!string.IsNullOrWhiteSpace(cityLine))
                                {
                                    c.Item().Text(cityLine);
                                }
                            });

                            row.RelativeItem(1).Column(c =>
                            {
                                c.Item().Text("Date").Bold();
                                c.Item().Text(quote.QuoteDate.ToString("dd MMM yyyy"));

                                c.Item().PaddingTop(6).Text("Expiry").Bold();
                                c.Item().Text(quote.ValidUntil?.ToString("dd MMM yyyy") ?? "-");

                                c.Item().PaddingTop(6).Text("Quote Number").Bold();
                                c.Item().Text(quote.ReferenceNumber);
                            });

                            row.RelativeItem(1).Column(c =>
                            {
                                c.Item().Text(salesRepName).Bold();
                                c.Item().Text(_companyInfo.Name);
                                c.Item().Text(_companyInfo.AddressLine1);
                                c.Item().Text(_companyInfo.AddressLine2);
                                c.Item().Text(_companyInfo.Country);
                                c.Item().Text(_companyInfo.Phone);
                            });
                        });

                        //Line items table
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(90);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("Description");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Quantity");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Unit Price");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Amount ZAR");

                                static IContainer HeaderStyle(IContainer c) => c
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Black)
                                    .PaddingBottom(5)
                                    .DefaultTextStyle(x => x.Bold());
                            });

                            foreach (var item in quote.QuoteItems)
                            {
                                var productName = products.TryGetValue(item.ProductID, out var name) ? name : "Unknown Product";

                                table.Cell().Element(RowStyle).Text(productName);
                                table.Cell().Element(RowStyle).AlignRight().Text(item.Quantity.ToString("N2"));
                                table.Cell().Element(RowStyle).AlignRight().Text(item.UnitPrice.ToString("N2"));
                                table.Cell().Element(RowStyle).AlignRight().Text(item.LineTotal.ToString("N2"));

                                static IContainer RowStyle(IContainer c) => c
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Grey.Lighten2)
                                    .PaddingVertical(6);
                            }
                        });

                        //Totals - VAT is disclosure only, TotalAmount is the actual amount owed either way.
                        col.Item().AlignRight().Width(260).Column(c =>
                        {
                            c.Item().BorderTop(1).BorderColor(Colors.Black).PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text("INCLUDES VAT");
                                r.RelativeItem().AlignRight().Text(quote.VatAmount.ToString("N2"));
                            });
                            c.Item().PaddingTop(3).Row(r =>
                            {
                                r.RelativeItem().Text("TOTAL ZAR").Bold();
                                r.RelativeItem().AlignRight().Text(quote.TotalAmount.ToString("N2")).Bold();
                            });
                        });

                        if (!string.IsNullOrWhiteSpace(quote.Notes))
                        {
                            col.Item().Column(c =>
                            {
                                c.Item().Text("Notes").Bold();
                                c.Item().Text(quote.Notes);
                            });
                        }

                        //Terms - company banking details, matching the sample layout.
                        col.Item().PaddingTop(10).Column(c =>
                        {
                            c.Item().BorderBottom(1).BorderColor(Colors.Black).PaddingBottom(3).Text("Terms").Bold();
                            c.Item().PaddingTop(6).Text(_companyInfo.Name);
                            c.Item().Text(_companyInfo.BankName);
                            c.Item().Text($"Account Number : {_companyInfo.AccountNumber}");
                            c.Item().Text($"Branch Code : {_companyInfo.BranchCode}");
                        });
                    });

                    page.Footer().PaddingTop(10).Text(_companyInfo.RegisteredOffice)
                        .FontSize(7).FontColor(Colors.Grey.Medium);
                });
            });

            return document.GeneratePdf();
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------