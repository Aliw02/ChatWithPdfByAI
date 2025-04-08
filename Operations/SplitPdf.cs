using Telegram.Bot.Types;
using Telegram.Bot;

using PdfSharpDocument = PdfSharp.Pdf.PdfDocument;

using SpirPdf = Spire.Pdf.PdfDocument;
using SpirDoc = Spire.Doc.Document;

 
 


using IronPDF = IronPdf.PdfDocument;
using IronXLFormat = IronXL.ExcelFileFormat;
using Telegram.Bot.Types.Enums;
using PDF_TelegramBot.System;



namespace PDF_TelegramBot.Operations
{
    public class SplitPdf
    {
        public static int From { get; set; } = -1;
        public static int To { get; set; } = -1;

        private static IronPDF? PdfToSplit;

        public static bool IsSplitProcess { get; set; } = false;

        public static async Task Split(ITelegramBotClient bot, UserSession userSession, ChatId chatId, CancellationToken token)
        {
            try
            {
                if (userSession.FileName is null || !userSession.FileName.EndsWith(".pdf"))
                {
                    await bot.SendMessage(chatId, "⚠️ قم بأرسال ملف رجاءً يجب ان يكون ملف PDF حصراً", cancellationToken: token);
                    throw new Exception();
                }

                IronPdf.License.LicenseKey = Requirements.Licence;

                ArgumentNullException.ThrowIfNull(userSession.TGFile);

                if (From is -1)
                {
                    await bot.SendMessage(chatId, "قم بإرسال الباراميتر الأول 'من'⚠️", cancellationToken: token);
                    IsSplitProcess = true;
                    var pdfStream = new MemoryStream();
                    await bot.DownloadFile(userSession.TGFile, pdfStream, token);
                    pdfStream.Position = 0;

                    PdfToSplit = PDF.CreateNewIronPdfFromPdfStream(pdfStream);
                    return;
                }

                if (To is -1 && IsSplitProcess)
                {
                    await bot.SendMessage(chatId, "قم بإرسال الباراميتر الثاني 'إلى' من فضلك ⚠️\nأرسل -1 لألغاء عملية الفصل⚠️", cancellationToken: token);
                    return;
                }

                if (!IsSplitProcess || PdfToSplit is null)
                {
                    await bot.SendMessage(chatId, "حدث خطأ ما قم بأعادة المحاولة رجاءاً ⚠️", cancellationToken: token);
                    return;
                }
                
                if (PdfToSplit.PageCount == 0)
                {
                    await bot.SendMessage(chatId, "⚠️ الملف PDF لا يحتوي على صفحات.", cancellationToken: token);
                    return;
                }

                if (From > To)
                {
                    await bot.SendMessage(chatId, "⚠️ رقم الصفحة 'من' لا يمكن أن يكون أكبر من رقم الصفحة 'إلى'.", cancellationToken: token);
                    To = -1;
                    return;
                }

                if (From >= PdfToSplit.PageCount)
                {
                    await bot.SendMessage(chatId, "⚠️ رقم الصفحة 'من' خارج النطاق.", cancellationToken: token);
                    From = -1;
                    return;
                }

                if (To > PdfToSplit.PageCount)
                {
                    await bot.SendMessage(chatId, "⚠️ رقم الصفحة 'إلى' خارج النطاق.", cancellationToken: token);
                    To = -1;
                    return;
                }


                // Split the PDF document
                var splitDocuments = PdfToSplit.CopyPages(From - 1, To - 1);


                await bot.SendChatAction(chatId, ChatAction.UploadDocument, cancellationToken: token);
                await bot.SendDocument(chatId, new InputFileStream(splitDocuments.Stream, $"{Path.GetFileNameWithoutExtension(userSession.FileName)}.pdf"), cancellationToken: token);
                await bot.SendMessage(chatId, "✅ تم اكمال العملية بنجاح", cancellationToken: token);
                //IsMergeProcess = false;
                From = -1;
                To = -1;
                IsSplitProcess = false;
            }
            catch (Exception ex)
            {
                await bot.SendMessage(chatId, $"❌ لقد حدث خطأ اثناء الفصل \n{ex.Message}", cancellationToken: token);
                IsSplitProcess = false;
                From = -1;
                To = -1;
            }
        }
    }
}
