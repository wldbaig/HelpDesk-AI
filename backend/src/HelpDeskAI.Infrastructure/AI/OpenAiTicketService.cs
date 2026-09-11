using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HelpDeskAI.Application.Common;
using HelpDeskAI.Application.Contracts;
using HelpDeskAI.Application.DTOs;
using Microsoft.Extensions.Options;

namespace HelpDeskAI.Infrastructure.AI;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";
    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "gpt-5.6-luna";
    public string BaseUrl { get; init; } = "https://api.openai.com/v1/";
}

public sealed class OpenAiTicketService(HttpClient httpClient, IOptions<OpenAiOptions> options) : IAiTicketService
{
    private readonly OpenAiOptions _options = options.Value;

    public async Task<AiAnalysisResult> AnalyzeAsync(string title, string description, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new ExternalServiceException("OpenAI is not configured. Set OpenAI:ApiKey using user-secrets or OPENAI__APIKEY.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = JsonContent.Create(new
        {
            model = _options.Model,
            messages = new object[]
            {
                new { role = "developer", content = "You are a customer-support triage assistant. Analyze only the supplied ticket. Choose exactly one allowed category and sentiment. Write a concise, professional reply that acknowledges the issue, avoids invented facts, and gives a clear next step." },
                new { role = "user", content = $"Ticket title: {title}\nTicket description: {description}" }
            },
            response_format = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "ticket_analysis",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        properties = new
                        {
                            category = new { type = "string", @enum = new[] { "Billing", "Technical", "Account", "Other" } },
                            sentiment = new { type = "string", @enum = new[] { "Positive", "Neutral", "Negative" } },
                            suggestedReply = new { type = "string" }
                        },
                        required = new[] { "category", "sentiment", "suggestedReply" },
                        additionalProperties = false
                    }
                }
            }
        });

        try
        {
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new ExternalServiceException($"OpenAI analysis failed with status {(int)response.StatusCode}.");

            using var document = JsonDocument.Parse(body);
            var content = document.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            return JsonSerializer.Deserialize<AiAnalysisResult>(content!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new ExternalServiceException("OpenAI returned an empty analysis.");
        }
        catch (ExternalServiceException) { throw; }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or KeyNotFoundException)
        {
            throw new ExternalServiceException("OpenAI returned an invalid or unavailable response.", ex);
        }
    }
}
