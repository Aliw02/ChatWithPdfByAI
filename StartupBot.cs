using dotenv.net; // لـ .env
using PDF_TelegramBot.System;
using Telegram.Bot; // Requirements, ClientSession

var builder = WebApplication.CreateBuilder(args);

// 1) Load المتغيّرات من .env
DotEnv.Load();

// 2) سجّل الـ BackgroundService اللي يشغّل البوت
builder.Services.AddHostedService<TelegramBotService>();

var app = builder.Build();

var botClient = Requirements.BotClient;
// 3) Healthcheck endpoint

if (botClient is null)
{
    Console.WriteLine("BotClient is null. Please check your bot token.");
    return;
}


var botInfo = await botClient.GetMe();
        //Console.WriteLine($"The Bot {botInfo.Username} is running right now...");

// 3) Healthcheck endpoint
app.MapGet("/", () => Results.Text("✅ Bot is running"));


// 4) شغّل السيرفر على 0.0.0.0:PORT
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");