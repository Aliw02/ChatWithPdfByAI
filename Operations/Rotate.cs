

using PdfSharpDocument = PdfSharp.Pdf.PdfDocument;

using SpirPdf = Spire.Pdf.PdfDocument;
using SpirDoc = Spire.Doc.Document;

using IronPDF = IronPdf.PdfDocument;
using IronXLFormat = IronXL.ExcelFileFormat;
using Orientation = IronPdf.Rendering.PdfPaperOrientation;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Spire.Pdf;
using PDF_TelegramBot.System;


namespace PDF_TelegramBot.Operations
{
    public class Rotate
    {
        public static async Task RotatePdfAsync(ITelegramBotClient bot, UserSession userSession, ChatId chatId, CancellationToken token)
        {
            try
            {
                // Validate user session file
                //if (string.IsNullOrEmpty(userSession.FileName) || !userSession.FileName.EndsWith(".pdf"))
                //{
                //    await bot.SendMessage(chatId, "⚠️ يرجى إرسال ملف PDF فقط", cancellationToken: token);
                //    return;
                //}

                //IronPdf.License.LicenseKey = Requirements.Licence;

                //if (userSession.TGFile is null)
                //{
                //    await bot.SendMessage(chatId, "⚠️ لا يوجد ملف لتحويله", cancellationToken: token);
                //    return;
                //}

                //using var pdfStream = new MemoryStream();
                //await bot.DownloadFile(userSession.TGFile, pdfStream, token);
                //pdfStream.Position = 0;


                //var rendered = IronPdf.;

                //rendered.RenderingOptions.PaperOrientation = Orientation.Landscape;
                //var pdfDoc = new IronPDF(pdfStream);



                //await bot.SendChatAction(chatId, ChatAction.UploadDocument, cancellationToken: token);
                //await bot.SendDocument(chatId, new InputFileStream(pdfDoc.Stream, $"{Path.GetFileNameWithoutExtension(userSession.FileName)}_rotated.pdf"), cancellationToken: token);
                //await bot.SendMessage(chatId, "✅ تمت العملية بنجاح", cancellationToken: token);
                await bot.SendMessage(chatId, " ستتوفر هذه الخاصية قريباً ✅", cancellationToken: token);
            }
            catch (Exception ex)
            {
                await bot.SendMessage(chatId, $"❌ حدث خطأ أثناء الدوران: {ex.Message}", cancellationToken: token);
            }
        }
    }
}
