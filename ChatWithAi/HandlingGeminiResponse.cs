using PDF_TelegramBot.System;

namespace PDF_TelegramBot.ChatWithAi
{
    public class HandlingGeminiResponse : GeminiResponse
    {
        public static async Task<GeminiResponse> HandleGptResponse(string Text, UserSession session)
        {
            try
            {
                string prompt;

                if (session.Pdf is null)
                {
                    return new GeminiResponse
                    {
                        IsSuccessful = false,
                        ErrorMessage = "لم يتم تحميل ملف PDF."
                    };
                }

                // نستخدم النص الذي كتبه المستخدم (السؤال)
                prompt = Text.StartsWith("/ask") ? Text["/ask".Length..].Trim() : Text is "" ? "اقرا الملف الوارد:" : Text;

                session.Prompt = prompt;
                return await AskAsync(session);
            }
            catch (Exception ex)
            {
                return new GeminiResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
