using Telegram.Bot.Types;
using Telegram.Bot;

using IronPDF = IronPdf.PdfDocument;
using Telegram.Bot.Types.Enums;
using PDF_TelegramBot.System;

namespace PDF_TelegramBot.Operations
{
    public class Security
    {
        public static string? Password {  get; set; }
        public static bool IsPerformPasswordProcess {  get; set; } = false;
        public static bool IsRemovePasswordProcess {  get; set; } = false;
        

        public static async Task Lock(ITelegramBotClient bot, UserSession userSession, ChatId chatId, CancellationToken token)
        {
            try
            {
                if (userSession.FileName is null || !userSession.FileName.EndsWith(".pdf"))
                {
                    await bot.SendMessage(chatId, "⚠️ قم بأرسال ملف رجاءً يجب ان يكون ملف PDF حصراً", cancellationToken: token);
                    throw new Exception();
                }

                if(Password is null)
                {
                    await bot.SendMessage(chatId, "أرسل كلمة المرور من فضلك: ", cancellationToken: token);
                    IsPerformPasswordProcess = true;
                    return;
                }
                IronPdf.License.LicenseKey = Requirements.Licence;

                ArgumentNullException.ThrowIfNull(userSession.TGFile);

                var pdfStream = new MemoryStream();
                await bot.DownloadFile(userSession.TGFile, pdfStream, token);
                pdfStream.Position = 0;

                var pdf = PDF.CreateNewIronPdfFromPdfStream(pdfStream);

                pdf.SecuritySettings.OwnerPassword = Password;
                pdf.SecuritySettings.UserPassword = Password;

                await bot.SendChatAction(chatId, ChatAction.UploadDocument, cancellationToken: token);
                await bot.SendDocument(chatId, new InputFileStream(pdf.Stream, $"{Path.GetFileNameWithoutExtension(userSession.FileName)}.pdf"), cancellationToken: token);
                await bot.SendMessage(chatId, "✅ تم اكمال العملية بنجاح", cancellationToken: token);
                IsPerformPasswordProcess = false;
                Password = null;
            }
            catch (Exception ex)
            {
                await bot.SendMessage(chatId, $"❌ لقد حدث خطأ اثناء الفصل \n{ex.Message}", cancellationToken: token);
                IsPerformPasswordProcess = false;
                Password = null;
            }
        }

        public static async Task Unlock(ITelegramBotClient bot, UserSession userSession, ChatId chatId, CancellationToken token)
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

                var pdfStream = new MemoryStream();
                await bot.DownloadFile(userSession.TGFile, pdfStream, token);
                pdfStream.Position = 0;

                if (Password is null)
                {
                    await bot.SendMessage(chatId, "أرسل كلمة المرور من فضلك: ", cancellationToken: token);
                    //await RequestPasswordReset(bot, userSession, chatId, token);
                    IsRemovePasswordProcess = true;
                    return;
                }

                var pdf = new IronPDF(pdfStream, Password);

                pdf.SecuritySettings.RemovePasswordsAndEncryption();

                await bot.SendChatAction(chatId, ChatAction.UploadDocument, cancellationToken: token);
                await bot.SendDocument(chatId, new InputFileStream(pdf.Stream, $"{Path.GetFileNameWithoutExtension(userSession.FileName)}.pdf"), cancellationToken: token);
                await bot.SendMessage(chatId, "✅ تم اكمال العملية بنجاح", cancellationToken: token);
                IsRemovePasswordProcess = false;
                Password = null;
            }
            catch (Exception ex)
            {
                await bot.SendMessage(chatId, $"❌ لقد حدث خطأ اثناء الفصل \n{ex.Message}", cancellationToken: token);
                IsRemovePasswordProcess = false;
                Password = null;
            }
        }

        //public static async Task ResetPassword(ITelegramBotClient bot, UserSession userSession, ChatId chatId, string providedToken, string newPassword, CancellationToken token)
        //{
        //    // Verify the reset token
        //    if (userSession.ResetToken != providedToken)
        //    {
        //        await bot.SendMessage(chatId, "Invalid reset token. Please try again.", cancellationToken: token);
        //        return;
        //    }

        //    // Set the new password
        //    Password = newPassword;

        //    // Clear the reset token
        //    userSession.ResetToken = null;

        //    await Unlock(bot, userSession, chatId, token);
        //    // Notify the user
        //    //await bot.SendMessage(chatId, "Your password has been reset successfully.", cancellationToken: token);
        //}

        //public static async Task RequestPasswordReset(ITelegramBotClient bot, UserSession userSession, ChatId chatId, CancellationToken token)
        //{
        //    // Generate a reset token
        //    var resetToken = Guid.NewGuid().ToString();

        //    // Store the reset token (this could be in a database or in-memory storage)
        //    userSession.ResetToken = resetToken;

        //    // Send the reset token to the user
        //    await bot.SendMessage(chatId, $"To reset your password, use the following token: {resetToken}", cancellationToken: token);

        //}

    }
}
