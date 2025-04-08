
using SpirPdf = Spire.Pdf.PdfDocument;
using SpirDoc = Spire.Doc.Document;

using IronPDF = IronPdf.PdfDocument;

using quest = QuestPDF.Fluent.Document;


using IronXL;
using IronXL.Options;
using System.Text;


namespace PDF_TelegramBot.System
{
    public class PDF
    {
        public static SpirPdf CreateNewSpirePdf()
        {
            return new SpirPdf();
        }

        public static SpirDoc CreateNewSpireDoc()
        {
            return new SpirDoc();
        }

        public static IronPDF CreateNewIronPdfFromExcelStream(MemoryStream excelStream)
        {
            // Set license key
            IronPdf.License.LicenseKey = Requirements.Licence;

            try
            {
                // Load Excel file with IronXL
                excelStream.Position = 0;
                IronXL.License.LicenseKey = IronPdf.License.LicenseKey; // Use same license key

                var options = new HtmlExportOptions
                {
                    OutputColumnHeaders = false,
                    OutputRowNumbers = false,
                    OutputHiddenColumns = false,
                    OutputHiddenRows = false,
                    OutputLeadingSpacesAsNonBreaking = false
                };

                var workbook = WorkBook.LoadExcel(excelStream).ExportToHtmlString(options);


                // Render HTML to PDF
                var renderer = new ChromePdfRenderer();
                renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A2;
                renderer.RenderingOptions.InputEncoding = Encoding.UTF8;
                renderer.RenderingOptions.PaperFit.UseResponsiveCssRendering();
                renderer.RenderingOptions.GrayScale = true;
                renderer.RenderingOptions.MarginTop = 10;
                renderer.RenderingOptions.MarginBottom = 10;
                renderer.RenderingOptions.MarginLeft = 10;
                renderer.RenderingOptions.MarginRight = 10;

                var pdf = renderer.RenderHtmlAsPdf(workbook);
                

                return pdf;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating PDF with IronPDF: {ex.Message}", ex);
            }
        }

        public static IronPDF CreateNewIronPdfFromPdfStream(MemoryStream pdfStream)
        {
            return new IronPDF(pdfStream);
        }

        public static IronPDF CreateNewIronPdfFromHtmlStream(MemoryStream htmlStream)
        {
            IronPdf.License.LicenseKey = Requirements.Licence;

            File.WriteAllBytes("convert.html", htmlStream.ToArray());
            return new ChromePdfRenderer().RenderHtmlFileAsPdf("convert.html");
        }

        public static IronPDF CreateNewIronPdfFromUrl(string url)
        {
            // Set license key
            IronPdf.License.LicenseKey = Requirements.Licence;
            return new ChromePdfRenderer().RenderUrlAsPdf(url);
        }

    }
}
