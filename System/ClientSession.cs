using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot.Types;



namespace PDF_TelegramBot.System
{
    public enum Language
    {
        English,
        Arabic
    }

    public class UserSession
    {
        public TGFile? TGFile { get; set; }
        public string? FileName { get; set; }
        public string? FullName { get; set; } 
        public string? Username { get; set; }
        public long  ChatID { get; set; }
        public double Id { get; set; }
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public DocumentState State { get; set; } = DocumentState.None;
        public Language Language { get; set; }
        public string? ResetToken { get; set; } // Add this property to st
        [JsonIgnore]
        public SemaphoreSlim ProcessingLock { get; } = new(1, 1);

        // داخل UserSession
        [JsonIgnore]
        public List<object> ConversationHistory { get; set; } = []; // سجل المحادثة
        public IronPdf.PdfDocument? Pdf { get; set; }                  // ملف PDF الخاص بالمستخدم
        public string Prompt { get; set; } = string.Empty;             // آخر سؤال للمستخدم
        public int MaxTokens { get; set; } = 800;
        public double Temperature { get; set; } = 1.0;
        public bool IsAiEnabled { get; set; } = false; // لتفعيل أو تعطيل الذكاء الاصطناعي
    }

    public enum DocumentState
    {
        None,
        WaitingForDocument,
        DocumentReceived,
        Processing,
        Completed
    }

    public class ClientSession
    {
        public static readonly ConcurrentDictionary<long, UserSession> userSessions = new();
        private static readonly TimeSpan SESSION_TIMEOUT = TimeSpan.FromMinutes(30);

        public static UserSession GetOrCreateSession(long chatId)
        {
            return userSessions.GetOrAdd(chatId, _ => new UserSession());
        }

        public static void UpdateSessionActivity(long chatId)
        {
            if (userSessions.TryGetValue(chatId, out var session))
            {
                session.LastActivity = DateTime.UtcNow;
            }
        }

        public static async Task CleanupInactiveSessions()
        {
            while (true)
            {
                var now = DateTime.UtcNow;
                var inactiveSessions = userSessions
                    .Where(kvp => now - kvp.Value.LastActivity > SESSION_TIMEOUT)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var chatId in inactiveSessions)
                {
                    userSessions.TryRemove(chatId, out _);
                }

                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }


        public static void SaveUserData(UserSession user)
        {
            try
            {
                string filePath = "users.json";

                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, "[]");
                }

                string jsn = File.ReadAllText(filePath);

                // Check if JSON is valid
                List<UserSession> users;
                try
                {
                    users = JsonSerializer.Deserialize<List<UserSession>>(jsn) ?? new List<UserSession>();
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"\n\nJSON Deserialization Error: {ex.Message}");
                    users = []; // Reset to an empty list
                }

                // Check if the user already exists
                if (users.Any(u => u.Id == user.Id))
                {
                    return;
                }

                users.Add(user);

                string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveUserData: {ex.Message}");
            }
        }
    }
}
