using System.Net.Http.Json;
using System.Text.Json;
using HelpDeskAI.Application.Contracts;
using Microsoft.Extensions.Options;

namespace HelpDeskAI.Infrastructure.AI;

public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";
    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "gemini-2.5-flash";
    public string BaseUrl { get; init; } = "https://generativelanguage.googleapis.com/v1beta/";
}

/// <summary>Google Gemini uses generateContent with a structured responseSchema and an x-goog-api-key header.</summary>
public sealed class GeminiTicketService(HttpClient httpClient, IOptions<GeminiOptions> options) : AiTicketServiceBase(httpClient)
{
    private readonly GeminiOptions _options = options.Value;

    public override AiProvider Provider => AiProvider.Gemini;
    protected override string ProviderName => "Gemini";
    protected override string ApiKeyConfigPath => "Gemini:ApiKey";
    protected override string ApiKey => _options.ApiKey;

    protected override HttpRequestMessage CreateRequest(string userPrompt)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"models/{_options.Model}:generateContent");
        request.Headers.Add("x-goog-api-key", _options.ApiKey);
        request.Content = JsonContent.Create(new
        {
            system_instruction = new { parts = new[] { new { text = SystemPrompt } } },
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = userPrompt } } }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseSchema = new
                {
                    type = "OBJECT",
                    properties = new
                    {
                        category = new { type = "STRING", @enum = AllowedCategories },
                        sentiment = new { type = "STRING", @enum = AllowedSentiments },
                        suggestedReply = new { type = "STRING" }
                    },
                    required = new[] { "category", "sentiment", "suggestedReply" }
                }
            }
        });
        return request;
    }

    protected override string? ExtractContent(JsonDocument document) =>
        document.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
}
