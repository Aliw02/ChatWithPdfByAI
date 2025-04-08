using System;
using System.Collections.Generic;
using System.Linq;
using PDF_TelegramBot.System;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace PDF_TelegramBot.HandlingRequests
{
    public class HandleUpdates
    {
        public static Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            if (update.Message is not null || update.CallbackQuery is not null)
            {
                _ = Task.Run(async () =>
                {
                    long chatId;
                    ArgumentNullException.ThrowIfNull(update);

                    if (update.Message is null)
                        chatId = update.CallbackQuery.Message.Chat.Id;
                    else
                        chatId = update.Message.Chat.Id;

                    var userSession = ClientSession.GetOrCreateSession(chatId);
                    try
                    {
                        ClientSession.UpdateSessionActivity(chatId);
                        HandleExceptions.UserSession = userSession;
                        await userSession.ProcessingLock.WaitAsync(token);
                        if (update.Type is UpdateType.Message)
                        {
                            if (update.Message.Type is MessageType.Document)
                            {
                                await HandleTheDocumentRequest.HandleDocumentMessage(bot, update, userSession, token);
                            }
                            else if (update.Message.Type is MessageType.Text)
                            {
                                await HandleTheDocumentRequest.HandleChatMessage(bot, update, userSession, token);
                            }
                            else if (update.Message.Type is MessageType.Photo)
                            {
                                await HandleTheDocumentRequest.HandlePhotoMessage(bot, update, userSession, token);
                            }
                        }
                        else if (update.Type is UpdateType.CallbackQuery)
                        {
                            await HandleCallBackquery.HandleCallbackQuery(bot, update, userSession, token);
                        }
                    }
                    finally
                    {
                        userSession.ProcessingLock.Release();
                    }
                }, token);
            }

            return Task.CompletedTask;
        }
    }
}
