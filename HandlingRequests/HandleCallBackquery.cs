using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDF_TelegramBot.ChatWithAi;
using PDF_TelegramBot.HandlingRequests.ConvertRequest;
using PDF_TelegramBot.Operations;
using PDF_TelegramBot.System;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace PDF_TelegramBot.HandlingRequests
{
    public class HandleCallBackquery
    {

        // ===================================================   //
        // ============== HandleCallBackQuery ================  //
        // =================================================== //
        public static async Task SendMainKeyboard(ITelegramBotClient bot, long chatId, CancellationToken token)
        {
            var keyboard = new InlineKeyboardMarkup(
            [
                [InlineKeyboardButton.WithCallbackData("🔄 التحويل", "show_conversion_options")],
                [InlineKeyboardButton.WithCallbackData("🗂️ الدمج", "merge_pdfs")],
                [InlineKeyboardButton.WithCallbackData("✂️ الفصل", "split_pdf")],
                [InlineKeyboardButton.WithCallbackData("🔒 الأمان", "protect_pdf")],
                [InlineKeyboardButton.WithCallbackData("🔓 فك كلمة المرور", "unlock_pdf")],
                [InlineKeyboardButton.WithCallbackData("محادثة مع ملف الـ PDF بواسطة الذكاء الأصطناعي🤖", "chat_with_pdf_ai")],
                [InlineKeyboardButton.WithCallbackData("❌ إلغاء العملية", "cancel_operation")]
            ]);

            await bot.SendMessage(chatId, "🔹 اختر العملية التي تريد تنفيذها:", replyMarkup: keyboard, cancellationToken: token);
        }

        public static async Task HandleCallbackQuery(ITelegramBotClient bot, Update update, UserSession usersession, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(update);
            ArgumentNullException.ThrowIfNull(usersession);

            var callbackQuery = update.CallbackQuery;
            ArgumentNullException.ThrowIfNull(callbackQuery);

            if (callbackQuery.Message is null)
            {
                await bot.AnswerCallbackQuery(callbackQuery.Id, text: "هذه الرسالة لم تعد صالحة.", showAlert: true, cancellationToken: token);
                return;
            }

            //var chatId = update.Id;

            if (callbackQuery is null)
                return;

            long chatId = callbackQuery.Message.Chat.Id;
            try
            {
                if (callbackQuery.Data == "show_conversion_options")
                {
                    var keyboard = GetConversionKeyboard();
                    string caption = "🔹 اختر نوع التحويل:";
                    await bot.EditMessageText(chatId, callbackQuery.Message.MessageId, caption, replyMarkup: keyboard, cancellationToken: token);
                }
                else if (callbackQuery.Data == "cancel_operation" || callbackQuery.Data == "back_to_main")
                {
                    await bot.DeleteMessage(chatId, callbackQuery.Message.MessageId, token);
                    await SendMainKeyboard(bot, chatId, token);
                }
                else if (await HandleDocumentOperationsAsync(bot, usersession, chatId, callbackQuery, token) || await HandleConversionOptions(bot, callbackQuery, usersession, chatId, token))
                {
                    //await bot.SendMessage(chatId, "❌ خيار غير معروف، يرجى الاختيار من القائمة.", cancellationToken: token);

                }
                else
                    await bot.SendMessage(chatId, "❌ خيار غير معروف، يرجى الاختيار من القائمة.", cancellationToken: token);

            }
            catch (Exception ex)
            {
                await bot.SendMessage(chatId, ex.Message, cancellationToken: token);
            }
        }

        private static InlineKeyboardMarkup GetConversionKeyboard()
        {
            return new InlineKeyboardMarkup(
            [
                [InlineKeyboardButton.WithCallbackData("📄 PDF ➝ Word 📝", "convert_pdf_to_word")],
                [InlineKeyboardButton.WithCallbackData("📝 Word ➝ PDF 📄", "convert_word_to_pdf")],
                [InlineKeyboardButton.WithCallbackData("🖼️ Image ➝ PDF 📄", "convert_image_to_pdf")],
                [InlineKeyboardButton.WithCallbackData("📄 PDF ➝ Image 🖼️", "convert_pdf_to_image")],
                [InlineKeyboardButton.WithCallbackData("📊 Excel ➝ PDF 📄", "convert_excel_to_pdf")],
                [InlineKeyboardButton.WithCallbackData("🌐 HTML ➝ PDF 📄", "convert_html_to_pdf")],
                [InlineKeyboardButton.WithCallbackData("🌐 URL ➝ PDF 📄", "convert_url_to_pdf")],
                [InlineKeyboardButton.WithCallbackData("⬅️ العودة إلى القائمة الرئيسية", "back_to_main")],
                [InlineKeyboardButton.WithCallbackData("❌ إلغاء العملية", "cancel_operation")]
            ]);
        }

        private static async Task<bool> HandleConversionOptions(ITelegramBotClient bot, CallbackQuery callbackQuery, UserSession usersession, ChatId chatId, CancellationToken token)
        {
            switch (callbackQuery.Data)
            {
                case "convert_pdf_to_word":
                    await FileConversionHandler.ConvertPdf2Word(bot, usersession, chatId, token);
                    return true;
                case "convert_word_to_pdf":
                    await FileConversionHandler.ConvertWord2Pdf(bot, usersession, chatId, token);
                    return true;
                case "convert_image_to_pdf":
                    await FileConversionHandler.ConvertImg2PDF(bot, usersession, chatId, token);
                    return true;
                case "convert_pdf_to_image":
                    await FileConversionHandler.ConvertPdf2Img(bot, usersession, chatId, token);
                    return true;
                case "convert_excel_to_pdf":
                    await FileConversionHandler.ConvertExcel2Pdf(bot, usersession, chatId, token);
                    return true;
                case "convert_html_to_pdf":
                    await FileConversionHandler.ConvertHtml2Pdf(bot, usersession, chatId, token);
                    return true;
                case "convert_url_to_pdf":
                    await bot.SendMessage(chatId, " يرجى إرسال رابط:", cancellationToken: token);
                    return true;
                default:
                    return false;
            }
        }

        private static async Task<bool> HandleDocumentOperationsAsync(ITelegramBotClient bot, UserSession usersession, ChatId chatId, CallbackQuery callbackQuery, CancellationToken token)
        {
            var data = callbackQuery.Data;

            switch (data)
            {
                case "merge_pdfs":
                    await MergePdfs.MergePdfsAsync(bot, usersession, chatId, token);
                    return true;

                case "split_pdf":
                    await SplitPdf.Split(bot, usersession, chatId, token);
                    return true;

                case "protect_pdf":
                    await Security.Lock(bot, usersession, chatId, token);
                    return true;

                case "unlock_pdf":
                    await Security.Unlock(bot, usersession, chatId, token);
                    return true;

                case "chat_with_pdf_ai":
                    {
                        GeminiResponse geminiResponse = new();
                        try
                        {
                            //GeminiRequirements.UserSession = usersession;

                            if (!usersession.IsAiEnabled)
                            {

                                await bot.SendMessage(chatId,
                                    "مرحبًا بك في وضع الدردشة مع الملف 📄\n\n" +
                                    "تستطيع سؤالي عن أي جزء من الملف وسأجيبك بناءً على محتواه فقط 🤖\n\n" +
                                    "• لتفعيل نمط المعلم أرسل: `علمني` أو `teachme` 👨‍🏫\n" +
                                    "  \\- سأبدأ بطرح الأسئلة عليك وتحليل إجاباتك بأسلوب تعليمي مستند إلى لغة الملف\n" +
                                    "  \\- لإيقافه أرسل: `كافي تعليم` أو `stopteachme` ❌\n\n" +
                                    "• لإلغاء العملية الحالية في أي وقت أرسل: `الغاء` أو `Cancel` ❌",
                                    parseMode: ParseMode.MarkdownV2,
                                    cancellationToken: token);

                            }

                            usersession.IsAiEnabled = true;
                            usersession.ConversationHistory.Clear();
                            ArgumentNullException.ThrowIfNull(usersession.TGFile);
                            var pdfStream = new MemoryStream();
                            await bot.DownloadFile(usersession.TGFile, pdfStream, token);
                            pdfStream.Position = 0;
                            usersession.Pdf = PDF.CreateNewIronPdfFromPdfStream(pdfStream);
                            geminiResponse = await HandlingGeminiResponse.HandleGptResponse("", usersession);

                            if (geminiResponse.IsSuccessful)
                                await bot.SendMessage(chatId, $"🤖\n {geminiResponse.Answer}\n{string.Join("\n", geminiResponse.SuggestedQuestions)}", parseMode: ParseMode.Html, cancellationToken: token);
                            else
                                await bot.SendMessage(chatId, $"حدث خطأ أثناء معالجة الملف:\n{geminiResponse.ErrorMessage} ❌", cancellationToken: token);

                        }
                        catch (Exception ex)
                        {
                            await bot.SendMessage(chatId, $"حدث خطأ أثناء معالجة الملف:\n{ex.Message}❌ ", cancellationToken: token);

                        }
                        return true;

                    }
                default: return false;

            }
        }


    }
}
