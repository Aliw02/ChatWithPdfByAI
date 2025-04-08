using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


namespace PDF_TelegramBot.System
{
    public static class Requirements
    {
        public static readonly string? BotToken = Environment.GetEnvironmentVariable("APISETTINGS__APITOKEN");
        public static readonly string? Licence = Environment.GetEnvironmentVariable("APISETTINGS__LICENCE");
        public static readonly string? GeminiApi = Environment.GetEnvironmentVariable("APISETTINGS__GEMINIAPI");

        public static TelegramBotClient? BotClient { get; private set; } = BotToken is not null ? new TelegramBotClient(BotToken) : null;

        public static readonly string ContactInfo = "🔹 <a href=\"https://www.facebook.com/ali.abood.94009841?mibextid=kFxxJD\">📘 فيسبوك</a>\n"
                                                  + "🔹 <a href=\"https://t.me/A2_Ab\">✈️ تيليجرام</a>\n"
                                                  + "🔹 <a href=\"https://www.instagram.com/abnaboodcode?igsh=a2ZsdHM0eHM4enVt\">📸 إنستغرام</a>";

        public static readonly InlineKeyboardMarkup ContactButtons = 
            new ([
                    [
                        InlineKeyboardButton.WithUrl("📩 تواصل معي على تيليجرام", "https://t.me/A2_Ab")
                    ],
                    [
                        InlineKeyboardButton.WithUrl("📘 صفحتي على فيسبوك", "https://www.facebook.com/ali.abood.94009841?mibextid=kFxxJD")
                    ],
                    [
                        InlineKeyboardButton.WithUrl("📸 حسابي على إنستغرام", "https://www.instagram.com/abnaboodcode?igsh=a2ZsdHM0eHM4enVt")
                    ]
                ]);

        public static void StartReceiving
        (
                ITelegramBotClient bot,
                Func<ITelegramBotClient, Update, CancellationToken, Task> HandleReceiving,
                Func<ITelegramBotClient, Exception, CancellationToken, Task> HandlePollingError)
        {
            var re = new ReceiverOptions
            {
                AllowedUpdates =
                [
                    UpdateType.Message,
                    UpdateType.CallbackQuery,
                    UpdateType.Poll
                    
                ]
            };

            using var cts = new CancellationTokenSource();

            ArgumentNullException.ThrowIfNull(bot);

            bot.StartReceiving(HandleReceiving, HandlePollingError, re ,cts.Token);
        }
    }
}
