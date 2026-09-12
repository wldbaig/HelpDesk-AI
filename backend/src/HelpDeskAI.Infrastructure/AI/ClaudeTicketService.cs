using System.Net.Http.Json;
using System.Text.Json;
using HelpDeskAI.Application.Contracts;
using Microsoft.Extensions.Options;

namespace HelpDeskAI.Infrastructure.AI;

public sealed class ClaudeOptions
{
    public const string SectionName = "Claude";
    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "claude-sonnet-4-5";
    public string BaseUrl { get; init; } = "https://api.anthropic.com/";
    public string AnthropicVersion { get; init; } = "2023-06-01";
    public int MaxTokens { get; init; } = 1024;
}

/// <summary>
/// Anthropic Claude uses the Messages API. To get structured output we expose a single tool and
/// force the model to call it, then read the analysis from the tool_use input block.
/// </summary>
public sealed class ClaudeTicketService(HttpClient httpClient, IOptions<ClaudeOptions> options) : AiTicketServiceBase(httpClient)
{
    private const string ToolName = "record_ticket_analysis";
    private readonly ClaudeOptions _options = options.Value;

    public override AiProvider Provider => AiProvider.Claude;
    protected override string ProviderName => "Claude";
    protected override string ApiKeyConfigPath => "Claude:ApiKey";
    protected override string ApiKey => _options.ApiKey;

    protected override HttpRequestMessage CreateRequest(string userPrompt)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "v1/messages");
        request.Headers.Add("x-api-key", _options.ApiKey);
        request.Headers.Add("anthropic-version", _options.AnthropicVersion);
        request.Content = JsonContent.Create(new
        {
            model = _options.Model,
            max_tokens = _options.MaxTokens,
            system = SystemPrompt,
            tools = new[]
            {
                new
                {
                    name = ToolName,
                    description = "Record the triage analysis for the supplied support ticket.",
                    input_schema = OpenAiCompatibleSchema
                }
            },
            tool_choice = new { type = "tool", name = ToolName },
            messages = new[]
            {
                new { role = "user", content = userPrompt }
            }
        });
        return request;
    }

    protected override string? ExtractContent(JsonDocument document)
    {
        foreach (var block in document.RootElement.GetProperty("content").EnumerateArray())
        {
            if (block.TryGetProperty("type", out var type) && type.GetString() == "tool_use")
                return block.GetProperty("input").GetRawText();
        }
        return null;
    }
}
