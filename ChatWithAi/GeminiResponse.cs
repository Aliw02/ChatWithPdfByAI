using System.Text;
using System.Text.Json;
using PDF_TelegramBot.System;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace PDF_TelegramBot.ChatWithAi
{
    public class GeminiResponse
    {
        public string Answer { get; set; } = string.Empty;
        public string[] SuggestedQuestions { get; set; } = [];
        public bool IsSuccessful { get; set; } = false;
        public string? ErrorMessage { get; set; }

        private static readonly string? ApiKey = Requirements.GeminiApi;

        protected static async Task<GeminiResponse> AskAsync(UserSession usersession)
        {
            try
            {
                using HttpClient client = new();
                string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={ApiKey}";
                
                if(!HandlePdf2Image(usersession))
                {
                    return new GeminiResponse
                    {
                        IsSuccessful = false,
                        ErrorMessage = "حدث خطأ أثناء معالجة ملف PDF."
                    };
                }

                var requestBody = new
                {
                    contents = usersession.ConversationHistory,
                    generation_config = new
                    {
                        temperature = usersession.Temperature,
                        max_output_tokens = usersession.MaxTokens
                    }
                };

                var jsonRequest = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiUrl, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new GeminiResponse
                    {
                        IsSuccessful = false,
                        ErrorMessage = $"API Error: {jsonResponse}"
                    };
                }

                var parsedResponse = JsonSerializer.Deserialize<GptApiResponse>(jsonResponse);
                if (parsedResponse?.candidates == null || parsedResponse.candidates.Length == 0)
                {
                    return new GeminiResponse
                    {
                        IsSuccessful = false,
                        ErrorMessage = "لم يتم تلقي استجابة صالحة من Google Gemini API."
                    };
                }

                // نضيف رد Gemini للمحادثة
                var geminiResponseText = parsedResponse.candidates[0].content.parts[0].text;
                usersession.ConversationHistory.Add(new
                {
                    role = "model",
                    parts = new object[]
                    {
                        new { text = geminiResponseText }
                    }
                });

                return new GeminiResponse
                {
                    Answer = geminiResponseText,
                    SuggestedQuestions = GenerateSuggestedQuestions(),
                    IsSuccessful = true
                };
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

        private static string[] GenerateSuggestedQuestions()
        {
            return
            [
                "🔹 ما هي النقاط الرئيسية في هذا الملف؟",
                "📅 هل يحتوي هذا الملف على تواريخ مهمة؟",
                "📝 هل يمكنك تلخيص هذا الملف؟"
            ];
        }

        private static bool HandlePdf2Image(UserSession usersession)
        {
            if (usersession.Pdf is null)
            {
                return false;
            }

            try
            {
                if (usersession.ConversationHistory.Count == 0)
                {
                    var images = usersession.Pdf.ToPngImages("*");
                    List<MemoryStream> imageStreams = [];

                    foreach (var imageBytes in images)
                    {
                        var imageStream = new MemoryStream();
                        using (var img = SixLabors.ImageSharp.Image.Load(imageBytes))
                        {
                            img.Mutate(x =>
                            {
                                // 1. Resize (if necessary)
                                // You can try resizing to increase resolution
                                // x.Resize(img.Width * 2, img.Height * 2, Resampler.Bicubic);

                                // 2. Noise Reduction
                                // You can use a filter to reduce noise if the images contain noise
                                // x.GaussianBlur(0.7f); // Light noise reduction

                                // 3. Sharpening
                                // You can sharpen the image to increase clarity
                                x.GaussianSharpen(1.5f);

                                // 4. Brightness/Contrast Adjustment
                                // You can adjust brightness and contrast to improve clarity
                                x.Brightness(1.2f); // Slightly increase brightness
                                x.Contrast(1.2f);   // Slightly increase contrast

                                // 5. Grayscale (optional)
                                //x.Grayscale();
                            });

                            img.Save(imageStream, new PngEncoder());
                        }

                        imageStream.Position = 0;
                        imageStreams.Add(imageStream);
                    }

                    for (int i = 0; i < imageStreams.Count; i++)
                    {
                        var stream = imageStreams[i];
                        using var ms = new MemoryStream();
                        stream.CopyTo(ms);
                        string base64Image = Convert.ToBase64String(ms.ToArray());

                        usersession.ConversationHistory.Add(new
                        {
                            role = "user",
                            parts = new object[]
                            {
                                new
                                {
                                    inlineData = new
                                    {
                                        mimeType = "image/png",
                                        data = base64Image
                                    }
                                },
                                new
                                {
                                    text = $"{usersession.Prompt}"
                                }
                             }
                        });

                        if (i == 0)
                        {
                            usersession.ConversationHistory.Add(new
                            {
                                role = "user",
                                parts = new object[]
                                {
                                    new
                                    {
                                        text = $"{GeminiRequirements.ImageProcessingConstraints}"
                                    }
                                }
                            });
                        }

                    }

                    usersession.ConversationHistory.Add(new
                    {
                        role = "user",
                        parts = new object[]
                        {
                            new { text = GeminiRequirements.IdentityAndScopeConstraints }
                        }
                    });

                    usersession.ConversationHistory.Add(new
                    {
                        role = "user",
                        parts = new object[]
                        {
                            new { text = GeminiRequirements.UnderstandingAndAnalysisConstraints }
                        }
                    });

                    usersession.ConversationHistory.Add(new
                    {
                        role = "user",
                        parts = new object[]
                        {
                            new { text = GeminiRequirements.UserExperienceConstraints }
                        }
                    });

                    usersession.ConversationHistory.Add(new
                    {
                        role = "user",
                        parts = new object[]
                        {
                            new { text = GeminiRequirements.ResponseFormattingConstraints }
                        }
                    });

                    usersession.ConversationHistory.Add(new
                    {
                        role = "user",
                        parts = new object[]
                        {
                            new { text = GeminiRequirements.ContentSpecificConstraints }
                        }
                    });

                    return true;

                }
                else
                {

                    usersession.ConversationHistory.Add(new
                    {
                        role = "user",
                        parts = new object[] { new { text = usersession.Prompt } }
                    });

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private class GptApiResponse
        {
            public Candidate[] candidates { get; set; } = [];
        }

        private class Candidate
        {
            public Content content { get; set; } = new();
        }

        private class Content
        {
            public Part[] parts { get; set; } = [];
        }

        private class Part
        {
            public string text { get; set; } = string.Empty;
        }
    }
}
