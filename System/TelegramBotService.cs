using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using PDF_TelegramBot.System;
using PDF_TelegramBot.HandlingRequests;

namespace PDF_TelegramBot.System
{
    public class TelegramBotService : BackgroundService
    {
        private readonly ITelegramBotClient _botClient;

        public TelegramBotService()
        {
            // تأكد إن الـ TOKEN موجود
            _botClient = Requirements.BotClient
                ?? throw new InvalidOperationException("❌ TELEGRAM_BOT_TOKEN مو موجود أو ما انعمل Load");

            // نظّف الجلسات القديمة
            _ = ClientSession.CleanupInactiveSessions();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // ابدأ الاستقبال
            Requirements.StartReceiving(
                _botClient,
                HandleUpdates.HandleUpdateAsync,
                HandleExceptions.HandleErrorAsync);

            Console.WriteLine("🤖 Bot started");

            // خلي الخدمة تعيش إلى الأبد أو لين يجي إلغاء
            return Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}