using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using PDF_TelegramBot.Operations;
using PDF_TelegramBot.HandlingRequests.ConvertRequest;
using PDF_TelegramBot.System;
using PDF_TelegramBot.ChatWithAi;


namespace PDF_TelegramBot.HandlingRequests
{
    public class HandleTheDocumentRequest
    {
        // =====================================================   //
        // =============== HandleTextMessage ===================  //
        // ===================================================== //
        public static async Task HandleChatMessage(ITelegramBotClient bot, Update update, UserSession userSession, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(update.Message);
            ArgumentNullException.ThrowIfNull(update.Message.Text);

            long chatId = update.Message.Chat.Id;
            string messageText = update.Message.Text;
            var user = update.Message.From;

            ArgumentNullException.ThrowIfNull(user);

            userSession.FullName = $"{user.FirstName} {user.LastName}";
            userSession.Username = user.Username;
            userSession.ChatID = chatId;
            userSession.Id = user.Id;

            
            if (Security.IsPerformPasswordProcess)
            {
                Security.Password = messageText;
                await Security.Lock(bot, userSession, chatId, token);
                return;
            }

            if (Security.IsRemovePasswordProcess)
            {
                Security.Password = messageText;
                await Security.Unlock(bot, userSession, chatId, token);
                return;
            }

            await HandleTextMessage(bot, chatId, userSession, messageText, token);
        }
        private static async Task HandleTextMessage(ITelegramBotClient bot, long chatId, UserSession userSession, string messageText, CancellationToken token)
        {
            if ((messageText is not "Cancel" && messageText is not "الغاء") && (messageText.StartsWith("/ask") || userSession.IsAiEnabled))
            {
                try
                {
                    userSession.IsAiEnabled = true;

                    var gptresponse = await HandlingGeminiResponse.HandleGptResponse(messageText, userSession);

                    if (gptresponse.IsSuccessful)
                        await bot.SendMessage(chatId, "🤖\n" + string.Join("\n", gptresponse.Answer), parseMode: ParseMode.Html, cancellationToken: token);
                    else
                        await bot.SendMessage(chatId, $"حدث خطأ أثناء معالجة الملف:\n{gptresponse.ErrorMessage} ❌", cancellationToken: token);

                    await bot.SendMessage(chatId, "لإلغاء هذه العملية أرسل كلمة الغاء \\ Cancel ❌", cancellationToken: token);

                    return;
                }
                catch (Exception ex)
                {
                    await bot.SendMessage(chatId, $"حدث خطأ أثناء معالجة الملف:\n{ex.Message} ❌", cancellationToken: token);
                    return;
                }
            }
            if (SplitPdf.IsSplitProcess && int.TryParse(messageText, out int Range))
            {
                if (Range > 0)
                {
                    if (SplitPdf.From is -1) SplitPdf.From = Range;
                    else if (SplitPdf.To is -1) SplitPdf.To = Range;

                    await SplitPdf.Split(bot, userSession, chatId, token);
                }
                else
                {
                    try
                    {
                        userSession.ProcessingLock.Release();
                        await bot.SendMessage(chatId, $"تم الغاء عملية الفصل بنجاح", cancellationToken: token);
                        SplitPdf.IsSplitProcess = false;
                        SplitPdf.From = -1;
                        SplitPdf.To = -1;
                    }
                    catch
                    {
                        await bot.SendMessage(chatId, "❌ حدث خطأ اثناء الغاء العملية", cancellationToken: token);
                        userSession.TGFile = null;
                        SplitPdf.IsSplitProcess = false;
                        SplitPdf.From = -1;
                        SplitPdf.To = -1;
                    }
                }
            }
            else if (Uri.IsWellFormedUriString(messageText, UriKind.Absolute))
            {
                await FileConversionHandler.ConvertUrl2Pdf(bot, userSession, chatId, messageText, token);
            }
            else
            {
                switch (messageText)
                {
                    case "/start":
                        await bot.SendMessage(chatId, $"👋 مرحبًا! {userSession.FullName} أرسل الملف ثم اختر أحد الخيارات أدناه:", cancellationToken: token);
                        ClientSession.SaveUserData(userSession);
                        break;
                    case "/contact":
                        //await bot.SendMessage(chatId, $"{Requirements.ContactInfo} ", parseMode: ParseMode.Html, cancellationToken: token);
                        await bot.SendMessage(chatId, $"📞 <b>معلومات التواصل والدعم:</b>\n\n📌 اختر من الأزرار أدناه للتواصل السريع 👇", parseMode: ParseMode.Html, replyMarkup: Requirements.ContactButtons, cancellationToken: token);
                        break;
                    case "Cancel":
                        userSession.IsAiEnabled = false;
                        //GeminiRequirements.UserSession = null;
                        userSession.ConversationHistory.Clear();
                        await bot.SendMessage(chatId, "تم الغاء العملية", cancellationToken: token);
                        break;
                    case "الغاء":
                        userSession.IsAiEnabled = false;
                        //userSession.UserSes = null;
                        userSession.ConversationHistory.Clear();
                        await bot.SendMessage(chatId, "تم الغاء العملية", cancellationToken: token);
                        break;
                    default:
                        await bot.SendMessage(chatId, "❌ لم أفهم هذا الأمر. يرجى اختيار خيار من القائمة.", cancellationToken: token);
                        break;
                }
            } 
        }


        // =====================================================   //
        // =============== HandlePhotoMessage ==================  //
        // ===================================================== //
        public static async Task HandlePhotoMessage(ITelegramBotClient bot, Update update, UserSession userSession, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(update.Message);
            ArgumentNullException.ThrowIfNull(update.Message.Photo);

            var photo = update.Message.Photo;

            long chatId = update.Message.Chat.Id;
            userSession.FileName = "Image2Pdf.pdf";

            try
            {
                // Ensure there are photos in the array
                if (update.Message.Photo == null || update.Message.Photo.Length == 0)
                {
                    await bot.SendMessage(chatId, "❌ لم يتم العثور على صورة صالحة.", cancellationToken: token);
                    return;
                }

                // Get the largest photo
                var largestPhoto = update.Message.Photo.OrderByDescending(p => p.FileSize).FirstOrDefault();
                if (largestPhoto == null)
                {
                    await bot.SendMessage(chatId, "❌ لم يتم العثور على صورة صالحة.", cancellationToken: token);
                    return;
                }

                // Get the file from Telegram
                var file = await bot.GetFile(largestPhoto.FileId, token);
                userSession.TGFile = file;

                await FileConversionHandler.ConvertImg2PDF(bot, userSession, chatId, token);
            }
            catch (Exception ex)
            {
                await bot.SendMessage(chatId, $"❌ حدث خطأ أثناء تحميل الصورة:\n{ex.Message}", cancellationToken: token);
            }
        }



        // =====================================================   //
        // =============== HandleDocumentMessage ===============  //
        // ===================================================== //
        public static async Task HandleDocumentMessage(ITelegramBotClient bot, Update update, UserSession userSession, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(update.Message);
            ArgumentNullException.ThrowIfNull(update.Message.Document);
            var document = update.Message.Document;
            ArgumentNullException.ThrowIfNull(update);
            long chatId = update.Message.Chat.Id;
            var user = update.Message.From;

            ArgumentNullException.ThrowIfNull(user);

            userSession.TGFile = await bot.GetFile(document.FileId, token);
            userSession.FileName = document.FileName;
            userSession.State = DocumentState.DocumentReceived;

            if (user.LanguageCode is "ar")
                userSession.Language = Language.Arabic;
            else
                userSession.Language = Language.English;

            if (document.FileName != null &&
                !document.FileName.EndsWith(".pdf") &&
                !document.FileName.EndsWith(".docx") && !document.FileName.EndsWith(".doc") &&
                !document.FileName.EndsWith(".xlsx") && !document.FileName.EndsWith(".xls") &&
                !document.FileName.EndsWith(".jpg") && !document.FileName.EndsWith(".jpeg") && !document.FileName.EndsWith(".png")&&
                !document.FileName.EndsWith(".htm") && !document.FileName.EndsWith(".html"))
            {
                userSession.State = DocumentState.None;
                await bot.SendMessage(chatId, "❌ يرجى إرسال الملفات الموجودة ضمن قسم التحويل فقط.", cancellationToken: token);
                return;
            }

            if (!MergePdfs.IsMergeProcess)
                await HandleCallBackquery.SendMainKeyboard(bot, chatId, token);

            else
                await MergePdfs.MergePdfsAsync(bot, userSession, chatId, token);
        }

        }
}
