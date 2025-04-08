using PDF_TelegramBot.System;
using Telegram.Bot;
namespace PDF_TelegramBot.HandlingRequests
{
    public class HandleExceptions
    {

        public static UserSession? UserSession { get; set; } 

        public static async Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            if (UserSession is null) 
                return;

            // Log the exception
            await bot.SendMessage(chatId: UserSession.ChatID, text: $"خطأ ⚠️: {exception.Message}", cancellationToken: token);
            return;
        }
    }
}
