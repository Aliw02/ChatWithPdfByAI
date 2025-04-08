
using SpirPDF = Spire.Pdf.PdfDocument;

using SixLabors.ImageSharp.Formats.Png;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Drawing;
using PDF_TelegramBot.System;
using System.Text;

namespace PDF_TelegramBot.HandlingRequests.ConvertRequest
{
    public class FileConversionHandler
    {

        // =================================================   //
        // =============== HandleConvertType ===============  //
        // ================================================= //
        public static async Task ConvertPdf2Word(ITelegramBotClient bot, UserSession userSession, ChatId chatId, CancellationToken token)
        {
            try
            {
                Message sentMessage = await bot.SendMessage(chatId, "🔄 يتم تحويل الملف إلى Word...", cancellationToken: token);
                await bot.SendChatAction(chatId, ChatAction.Typing, cancellationToken: token);

                // التأكد من أن الملف موجود في الجلسة
                ArgumentNullException.ThrowIfNull(userSession.TGFile);

                using var pdfStream = new MemoryStream();
                await bot.DownloadFile(userSession.TGFile, pdfStream, token);
                pdfStream.Position = 0;

                // تحويل PDF إلى Word
                using var wordStream = new MemoryStream();

                var pdf = PDF.CreateNewSpirePdf();

                pdf.LoadFromStream(pdfStream);
                pdf.SaveToStream(wordStream, Spire.Pdf.FileFormat.DOCX);
                wordStream.Position = 0;

                await bot.EditMessageText(chatId, sentMessage.MessageId, "▒▒ يتم التحويل...", cancellationToken: token);
                await Task.Delay(1000, token);
                await bot.EditMessageText(chatId, sentMessage.MessageId, "░░ يتم التحويل...", cancellationToken: token);
                await Task.Delay(1000, token);

                // إرسال الملف المحول
                await bot.SendDocument(chatId, new InputFileStream(wordStream, $"{Path.GetFileNameWithoutExtension(userSession.FileName)}.docx"), cancellationToken: token);
                await bot.SendMessage(chatId, "✅ تم التحويل بنجاح!", cancellationToken: token);
                await Task.Delay(500, token);
                await bot.DeleteMessage(chatId, sentMessage.MessageId, cancellationToken: token);
            }
            catch (Exception ex)
            {
                await bot.SendMessage(chatId, $"\n❌ حدث خطأ أثناء تحويل الملف:\n{ex.Message}", cancellationToken: token);
            }
        }

        public static async Task ConvertWord2Pdf(ITelegramBotClient bot, UserSession userSession, ChatId chatId, CancellationToken token)
        {
            try
            {
                Message sentMessage = await bot.SendMessage(chatId, "🔄 يتم تحويل الملف إلى PDF...", cancellationToken: token);
                await bot.SendChatAction(chatId, ChatAction.Typing, cancellationToken: token);

                // التأكد من أن الملف موجود في الجلسة
                ArgumentNullException.ThrowIfNull(userSession.TGFile);

                using var wordStream = new MemoryStream();
                await bot.DownloadFile(userSession.TGFile, wordStream, token);
                wordStream.Position = 0;

                using var pdfStream = new MemoryStream();
                Spire.Doc.Document word = PDF.CreateNewSpireDoc();
                word.LoadFromStream(wordStream, Spire.Doc.FileFormat.Docx);
                word.SaveToStream(pdfStream, Spire.Doc.FileFormat.PDF);
                pdfStream.Position = 0;

                await bot.EditMessageText(chatId, sentMessage.MessageId, "▒▒ يتم التحويل...", cancellationToken: token);
                await Task.Delay(1000, token);
                await bot.EditMessageText(chatId, sentMessage.MessageId, "░░ يتم التحويل...", cancellationToken: token);
                await Task.Delay(1000, token);

                // إرسال الملف المحول
                await bot.SendDocument(chatId, new InputFileStream(pdfStream, $"{Path.GetFileNameWithoutExtension(userSession.FileName)}.pdf"), cancellationToken: token);
                await bot.SendMessage(chatId, "✅ تم التحويل بنجاح!", cancellationToken: token);
                await Task.Delay(500, token);
                await bot.DeleteMessage(chatId, sentMessage.MessageId, cancellationToken: token);
            }
            catch (Exception ex)
            {
                await bot.SendMessage(chatId, $"\n❌ حدث خطأ أثناء تحويل الملف:\n{ex.Message}", cancellationToken: token);
            }
        }



        public static async Task ConvertImg2PDF(ITelegramBotClient bot, UserSession userSession, ChatId chatId, CancellationToken token)
        {
            try
            {

                Message sentMessage = await bot.SendMessage(chatId, "🔄 يتم تحويل الصورة إلى PDF...", cancellationToken: token);
                await bot.SendChatAction(chatId, ChatAction.Typing, cancellationToken: token);

                // تحميل الصورة من تيليجرام
                ArgumentNullException.ThrowIfNull(userSession.TGFile);
                using var imageStream = new MemoryStream();
                await bot.DownloadFile(userSession.TGFile, imageStream, token);
                imageStream.Position = 0;

                ArgumentNullException.ThrowIfNull(imageStream);

                // Set license key
                License.LicenseKey = Requirements.Licence;
                
                var pdf = ImageToPdfConverter.ImageToPdf(new Bitmap(imageStream), IronPdf.Imaging.ImageBehavior.FitToPageAndMaintainAspectRatio);

                var pdfStream = pdf.Stream;

                await bot.SendDocument(chatId, new InputFileStream(pdfStream, $"{Path.GetFileNameWithoutExtension(userSession.FileName)}.pdf"), cancellationToken: token);
                await bot.SendMessage(chatId, "✅ تم تحويل الصورة إلى PDF بنجاح!", cancellationToken: token);
                await bot.DeleteMessage(chatId, sentMessage.MessageId, cancellationToken: token);

            }
            catch (Exception ex)
            {
                await bot.SendMessage(chatId, $"❌ حدث خطأ أثناء التحويل:\n{ex.Message}", cancellationToken: token);
            }

        }

        public static async Task ConvertPdf2Img(ITelegramBotClient bot, UserSession userSession, ChatId chatId, CancellationToken token)
        {
            try
            {
                Message sentMessage = await bot.SendMessage(chatId, "🔄 يتم استخراج الصور من PDF...", cancellationToken: token);
                await bot.SendChatAction(chatId, ChatAction.Typing, cancellationToken: token);

                // تحميل ملف PDF من تيليجرام
                ArgumentNullException.ThrowIfNull(userSession.TGFile);
                using var pdfStream = new MemoryStream();
                await bot.DownloadFile(userSession.TGFile, pdfStream, token);
                pdfStream.Position = 0;

                // تحويل PDF إلى صور باستخدام IronPdf
                var pdfDocument = PDF.CreateNewIronPdfFromPdfStream(pdfStream);
                var images = pdfDocument.ToPngImages("*");

                foreach (var image in images.Select((value, index) => new { value, index }))
                {
                    using var imageStream = new MemoryStream();
                    using (var img = SixLabors.ImageSharp.Image.Load(image.value))
                    {
                        img.Save(imageStream, new PngEncoder());
                    }
                    imageStream.Position = 0;

                    await bot.SendPhoto(chatId, new InputFileStream(imageStream, $"Page-{image.index + 1}.png"), cancellationToken: token);
                }


                await bot.SendMessage(chatId, "✅ تم استخراج الصور من PDF!", cancellationToken: token);
                await bot.DeleteMessage(chatId, sentMessage.MessageId, cancellationToken: token);
            }
            catch (Exception ex)
            {
                await bot.SendMessage(chatId, $"❌ حدث خطأ أثناء التحويل:\n{ex.Message}", cancellationToken: token);
            }
        }



        public static async Task ConvertExcel2Pdf(ITelegramBotClient bot, UserSession userSession, ChatId chatId, CancellationToken token)
        {
            try
            {
                Message sentMessage = await bot.SendMessage(chatId, "🔄 يتم تحويل Excel إلى PDF...", cancellationToken: token);

                // تحميل ملف Excel من تيليجرام
                ArgumentNullException.ThrowIfNull(userSession.TGFile);
                using var excelStream = new MemoryStream();
                await bot.DownloadFile(userSession.TGFile, excelStream, token);
                await Task.Delay(2500, token);


                await bot.EditMessageText(chatId, sentMessage.MessageId, "▒▒ يتم التحويل...", cancellationToken: token);
                await Task.Delay(1000, token);

                var pdfDocument = PDF.CreateNewIronPdfFromExcelStream(excelStream);
                await bot.EditMessageText(chatId, sentMessage.MessageId, "░░ يتم التحويل...", cancellationToken: token);
                await Task.Delay(1000, token);

                var pdfstream = new MemoryStream(pdfDocument.BinaryData);

                await bot.SendChatAction(chatId, ChatAction.UploadDocument, cancellationToken: token);
                await Task.Delay(3000, token);
                await bot.SendDocument(chatId, new InputFileStream(pdfstream, $"{Path.GetFileNameWithoutExtension(userSession.FileName)}.pdf"), cancellationToken: token);
                await bot.SendMessage(chatId, "✅ تم تحويل Excel إلى PDF!", cancellationToken: token);
                await bot.DeleteMessage(chatId, sentMessage.MessageId, cancellationToken: token);

            }
            catch (Exception ex)
            {
                await bot.SendMessage(
                    chatId,
                    $"❌ حدث خطأ أثناء التحويل:\n{ex.Message}",
                    cancellationToken: token);
            }
        }

        //public static async Task ConvertPdf2Excel(ITelegramBotClient bot, UserSession userSession, ChatId chatId, CancellationToken token)
        //{
        //    try
        //    {

        //        await bot.SendMessage(chatId, " ستتوفر قريباً..!!\n📄 PDF ➝ Excel 📊", cancellationToken: token);
        //    }
        //    catch (Exception ex)
        //    {
        //        await bot.SendMessage(chatId, $"\n❌ حدث خطأ أثناء تحويل الملف:\n{ex.Message}", cancellationToken: token);
        //    }
        //}


        public static async Task ConvertHtml2Pdf(ITelegramBotClient bot, UserSession userSession, ChatId chatId, CancellationToken token)
        {
            try
            {
                Message sentMessage = await bot.SendMessage(chatId, "يتم تحويل HTML إلى PDF...🔄", cancellationToken: token);

                // تحميل ملف Excel من تيليجرام
                ArgumentNullException.ThrowIfNull(userSession.TGFile);
                using var htmlStream = new MemoryStream();
                await bot.DownloadFile(userSession.TGFile, htmlStream, token);
                await Task.Delay(2500, token);
                htmlStream.Position = 0;


                await bot.EditMessageText(chatId, sentMessage.MessageId, "▒▒ يتم التحويل...", cancellationToken: token);
                await Task.Delay(1000, token);

                var pdfDocument = PDF.CreateNewIronPdfFromHtmlStream(htmlStream);
                await bot.EditMessageText(chatId, sentMessage.MessageId, "░░ يتم التحويل...", cancellationToken: token);
                await Task.Delay(1000, token);

                await bot.SendChatAction(chatId, ChatAction.UploadDocument, cancellationToken: token);
                await Task.Delay(3000, token);
                await bot.SendDocument(chatId, new InputFileStream(pdfDocument.Stream, $"{Path.GetFileNameWithoutExtension(userSession.FileName)}.pdf"), cancellationToken: token);
                await bot.SendMessage(chatId, "✅ تم تحويل Excel إلى PDF!", cancellationToken: token);
                await bot.DeleteMessage(chatId, sentMessage.MessageId, cancellationToken: token);
                File.Delete("conver.html");
            }
            catch (Exception ex)
            {
                await bot.SendMessage(chatId, $"\n❌ حدث خطأ أثناء تحويل الملف:\n{ex.Message}", cancellationToken: token);
            }

        }

        public static async Task ConvertUrl2Pdf(ITelegramBotClient bot, UserSession userSession, ChatId chatId, string url, CancellationToken token)
        {
            try
            {
                Message sentMessage = await bot.SendMessage(chatId, "يتم تحويل URL إلى PDF...🔄", cancellationToken: token);
                
                await Task.Delay(2500, token);


                await bot.EditMessageText(chatId, sentMessage.MessageId, "▒▒ يتم التحويل...", cancellationToken: token);
                await Task.Delay(1000, token);

                var pdf = PDF.CreateNewIronPdfFromUrl(url);

                await bot.EditMessageText(chatId, sentMessage.MessageId, "░░ يتم التحويل...", cancellationToken: token);
                await Task.Delay(1000, token);

                await bot.SendChatAction(chatId, ChatAction.UploadDocument, cancellationToken: token);
                await Task.Delay(3000, token);
                await bot.SendDocument(chatId, new InputFileStream(pdf.Stream, $"Url2Pdf.pdf"), cancellationToken: token);
                await bot.SendMessage(chatId, "✅ تم تحويل URL إلى PDF!", cancellationToken: token);
                await bot.DeleteMessage(chatId, sentMessage.MessageId, cancellationToken: token);


            }
            catch (Exception ex)
            {
                await bot.SendMessage(chatId, $"\n❌ حدث خطأ أثناء تحويل الرابط:\n{ex.Message}", cancellationToken: token);
            }
        }


    }
}
