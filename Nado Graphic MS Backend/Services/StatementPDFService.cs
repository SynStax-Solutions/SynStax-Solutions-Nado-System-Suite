using Microsoft.Extensions.Options;
using Nado_Graphics_MS_Backend.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Nado_Graphics_MS_Backend.Services
{
    // Generates the statement PDF. Mirrors InvoicePDFService so the documents look the same.
    public class StatementPDFService
    {
        private readonly StatementService _statementService;
        private readonly CompanyInfoOptions _companyInfo;
        private readonly IWebHostEnvironment _environment;

        public StatementPDFService(StatementService statementService, IOptions<CompanyInfoOptions> companyInfo, IWebHostEnvironment environment)
        {
            _statementService = statementService;
            _companyInfo = companyInfo.Value;
            _environment = environment;
        }
        //----------------------------------------------------------------------------------
        public async Task<byte[]?> GenerateStatementPdfAsync(int statementId, string salesRepName)
        {
            var data = await _statementService.GetStatementDataAsync(statementId);

            if (data == null)
            {
                return null;
            }

            var statement = data.Statement;
            var logoPath = Path.Combine(_environment.ContentRootPath, "Assets", "nado-logo.png");
            var hasLogo = File.Exists(logoPath);

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Text("STATEMENT").FontSize(28).Bold();

                        if (hasLogo)
                        {
                            row.ConstantItem(160).Image(logoPath);
                        }
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(15);

                        // Account holder left / statement details middle / staff member and company right
                        col.Item().Row(row =>
                        {
                            row.RelativeItem(2).Column(c =>
                            {
                                c.Item().Text(data.AccountName).Bold();

                                foreach (var line in data.AddressLines)
                                {
                                    c.Item().Text(line);
                                }
                            });

                            row.RelativeItem(1).Column(c =>
                            {
                                c.Item().Text("Statement Date").Bold();
                                c.Item().Text(statement.DateGenerated.ToString("dd MMM yyyy"));

                                c.Item().PaddingTop(6).Text("Period").Bold();
                                c.Item().Text($"{statement.StartDate:dd MMM yyyy} - {statement.EndDate:dd MMM yyyy}");

                                c.Item().PaddingTop(6).Text("Statement Number").Bold();
                                c.Item().Text(statement.StatementReference);
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

                        // Transactions with running balance
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(65);   // Date
                                columns.ConstantColumn(95);   // Reference
                                columns.RelativeColumn(3);    // Description
                                columns.ConstantColumn(70);   // Debit
                                columns.ConstantColumn(70);   // Credit
                                columns.ConstantColumn(75);   // Balance
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("Date");
                                header.Cell().Element(HeaderStyle).Text("Reference");
                                header.Cell().Element(HeaderStyle).Text("Description");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Debit");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Credit");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Balance ZAR");

                                static IContainer HeaderStyle(IContainer c) => c
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Black)
                                    .PaddingBottom(5)
                                    .DefaultTextStyle(x => x.Bold());
                            });

                            // Opening balance row
                            table.Cell().Element(RowStyle).Text(statement.StartDate.ToString("dd MMM yy"));
                            table.Cell().Element(RowStyle).Text("");
                            table.Cell().Element(RowStyle).Text("Opening Balance").Bold();
                            table.Cell().Element(RowStyle).AlignRight().Text("");
                            table.Cell().Element(RowStyle).AlignRight().Text("");
                            table.Cell().Element(RowStyle).AlignRight().Text(data.OpeningBalance.ToString("N2")).Bold();

                            foreach (var line in data.Lines)
                            {
                                table.Cell().Element(RowStyle).Text(line.Date.ToString("dd MMM yy"));
                                table.Cell().Element(RowStyle).Text(line.Reference);
                                table.Cell().Element(RowStyle).Text(line.Description);
                                table.Cell().Element(RowStyle).AlignRight().Text(line.Debit > 0 ? line.Debit.ToString("N2") : "");
                                table.Cell().Element(RowStyle).AlignRight().Text(line.Credit > 0 ? line.Credit.ToString("N2") : "");
                                table.Cell().Element(RowStyle).AlignRight().Text(line.Balance.ToString("N2"));
                            }

                            static IContainer RowStyle(IContainer c) => c
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(6);
                        });

                        // Totals
                        col.Item().AlignRight().Width(260).Column(c =>
                        {
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("OPENING BALANCE");
                                r.RelativeItem().AlignRight().Text(data.OpeningBalance.ToString("N2"));
                            });
                            c.Item().PaddingTop(3).Row(r =>
                            {
                                r.RelativeItem().Text("TOTAL INVOICED");
                                r.RelativeItem().AlignRight().Text(data.TotalInvoiced.ToString("N2"));
                            });
                            c.Item().PaddingTop(3).Row(r =>
                            {
                                r.RelativeItem().Text("TOTAL PAID");
                                r.RelativeItem().AlignRight().Text(data.TotalPaid.ToString("N2"));
                            });
                            c.Item().PaddingTop(3).BorderTop(1).BorderColor(Colors.Black).PaddingTop(3).Row(r =>
                            {
                                r.RelativeItem().Text("BALANCE DUE ZAR").Bold();
                                r.RelativeItem().AlignRight().Text(data.ClosingBalance.ToString("N2")).Bold();
                            });
                        });

                        // Payment details
                        col.Item().PaddingTop(10).Column(c =>
                        {
                            c.Item().BorderBottom(1).BorderColor(Colors.Black).PaddingBottom(3).Text("Payment Details").Bold();
                            c.Item().PaddingTop(6).Text(_companyInfo.Name);
                            c.Item().Text(_companyInfo.BankName);
                            c.Item().Text($"Account Number : {_companyInfo.AccountNumber}");
                            c.Item().Text($"Branch Code : {_companyInfo.BranchCode}");
                            c.Item().Text($"Payment Reference : {statement.StatementReference}");
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