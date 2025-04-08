using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using PdfSharpDocument = PdfSharp.Pdf.PdfDocument;

using SpirPdf = Spire.Pdf.PdfDocument;
using SpirDoc = Spire.Doc.Document;

using IronPDF = IronPdf.PdfDocument;
using IronXLFormat = IronXL.ExcelFileFormat;
using PDF_TelegramBot.System;


namespace PDF_TelegramBot.Operations
{
    public class MergePdfs
    {
        private static MemoryStream? APdfStream { get; set; } 
        private static MemoryStream? BPdfStream { get; set; }

        public static bool IsMergeProcess { get; set; } = false;

        public static async Task MergePdfsAsync(ITelegramBotClient bot, UserSession userSession, ChatId chatId, CancellationToken token, bool opposite = false)
        {
            try
            {
                if (userSession.FileName is null || !userSession.FileName.EndsWith(".pdf"))
                {
                    await bot.SendMessage(chatId, "⚠️ قم بأرسال ملف رجاءً يجب ان يكون ملف PDF حصراً", cancellationToken: token);
                    return;
                    //throw new Exception();
                }

                IronPdf.License.LicenseKey = Requirements.Licence;

                ArgumentNullException.ThrowIfNull(userSession.TGFile);

                if (APdfStream is null)
                {
                    APdfStream = new MemoryStream();
                    await bot.DownloadFile(userSession.TGFile, APdfStream, token);
                    await bot.SendMessage(chatId, "📄 قم بأرسال الملف الاّخر رجاءً يجب ان يكون ملف PDF حصراً", cancellationToken: token);
                    IsMergeProcess = true;
                    return;
                }
                else if (BPdfStream is null)
                {
                    BPdfStream = new MemoryStream();
                    await bot.DownloadFile(userSession.TGFile, BPdfStream, token);
                }

                APdfStream.Position = 0;
                BPdfStream.Position = 0;

                await bot.SendMessage(chatId, "⏳ جاري معالجة الملفات...", cancellationToken: token);
                await bot.SendChatAction(chatId, ChatAction.UploadDocument, cancellationToken: token);

                await Task.Delay(2000);

                IronPDF A = new(APdfStream);
                IronPDF B = new(BPdfStream);

                IronPDF C;
                if (!opposite)
                    C = IronPDF.Merge(A, B);
                else
                    C = IronPDF.Merge(B, A);

                await bot.SendChatAction(chatId, ChatAction.UploadDocument, cancellationToken: token);
                await bot.SendDocument(chatId, new InputFileStream(C.Stream, $"{Path.GetFileNameWithoutExtension(userSession.FileName)}.pdf"), cancellationToken: token);
                await bot.SendMessage(chatId, "✅ تم اكمال العملية بنجاح", cancellationToken: token);
                IsMergeProcess = false;
                APdfStream = null;
                BPdfStream = null;
            }
            catch (Exception ex)
            {
                await bot.SendMessage(chatId, $"❌ لقد حدث خطأ اثناء الدمج \n{ex.Message}", cancellationToken: token);
                IsMergeProcess = false;
                APdfStream = null;
                BPdfStream = null;
            }
        }

    }
}
