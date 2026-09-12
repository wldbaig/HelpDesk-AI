using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HelpDeskAI.Application.Contracts;
using Microsoft.Extensions.Options;

namespace HelpDeskAI.Infrastructure.AI;

public sealed class GrokOptions
{
    public const string SectionName = "Grok";
    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "grok-4";
    public string BaseUrl { get; init; } = "https://api.x.ai/v1/";
}

/// <summary>xAI Grok exposes an OpenAI-compatible chat-completions API, so the request shape matches OpenAI.</summary>
public sealed class GrokTicketService(HttpClient httpClient, IOptions<GrokOptions> options) : AiTicketServiceBase(httpClient)
{
    private readonly GrokOptions _options = options.Value;

    public override AiProvider Provider => AiProvider.Grok;
    protected override string ProviderName => "Grok";
    protected override string ApiKeyConfigPath => "Grok:ApiKey";
    protected override string ApiKey => _options.ApiKey;

    protected override HttpRequestMessage CreateRequest(string userPrompt)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = JsonContent.Create(new
        {
            model = _options.Model,
            messages = new object[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user", content = userPrompt }
            },
            response_format = new
            {
                type = "json_schema",
                json_schema = new { name = "ticket_analysis", strict = true, schema = OpenAiCompatibleSchema }
            }
        });
        return request;
    }

    protected override string? ExtractContent(JsonDocument document) =>
        document.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
}
