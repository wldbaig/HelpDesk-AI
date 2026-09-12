using System.Net;
using System.Text;
using System.Text.Json;
using HelpDeskAI.Application.Common;
using HelpDeskAI.Application.Contracts;
using HelpDeskAI.Application.DTOs;

namespace HelpDeskAI.Infrastructure.AI;

/// <summary>
/// Shared pipeline for every LLM-backed ticket analyzer: it validates that an API key is
/// configured, sends the provider-specific request, and turns the model output into a typed
/// <see cref="AiAnalysisResult"/>. Concrete providers only describe how to build the request
/// and where the JSON payload sits in their response.
/// </summary>
public abstract class AiTicketServiceBase(HttpClient httpClient) : IAiTicketService
{
    protected const string SystemPrompt =
        "You are a customer-support triage assistant. Analyze only the supplied ticket. " +
        "Choose exactly one allowed category and sentiment. Write a concise, professional reply " +
        "that acknowledges the issue, avoids invented facts, and gives a clear next step.";

    protected static readonly string[] AllowedCategories = ["Billing", "Technical", "Account", "Other"];
    protected static readonly string[] AllowedSentiments = ["Positive", "Neutral", "Negative"];

    private static readonly JsonSerializerOptions ResultJsonOptions = new() { PropertyNameCaseInsensitive = true };

    // Providers occasionally answer with a transient error (rate limit or overloaded);
    // retry a few times with a short backoff before surfacing the failure.
    private const int MaxAttempts = 3;
    private static readonly TimeSpan RetryBackoff = TimeSpan.FromSeconds(1.5);

    protected HttpClient HttpClient { get; } = httpClient;

    public abstract AiProvider Provider { get; }

    /// <summary>Human-readable provider name used in error messages.</summary>
    protected abstract string ProviderName { get; }

    /// <summary>Configuration key that supplies the API key, surfaced when it is missing.</summary>
    protected abstract string ApiKeyConfigPath { get; }

    /// <summary>The configured API key for this provider.</summary>
    protected abstract string ApiKey { get; }

    /// <summary>Builds the provider-specific HTTP request from the assembled user prompt.</summary>
    protected abstract HttpRequestMessage CreateRequest(string userPrompt);

    /// <summary>Extracts the raw JSON analysis payload from the provider response.</summary>
    protected abstract string? ExtractContent(JsonDocument document);

    /// <summary>The JSON schema shared by OpenAI-compatible providers (OpenAI, Grok).</summary>
    protected static object OpenAiCompatibleSchema => new
    {
        type = "object",
        properties = new
        {
            category = new { type = "string", @enum = AllowedCategories },
            sentiment = new { type = "string", @enum = AllowedSentiments },
            suggestedReply = new { type = "string" }
        },
        required = new[] { "category", "sentiment", "suggestedReply" },
        additionalProperties = false
    };

    protected static string BuildUserPrompt(string title, string description, IReadOnlyList<string> comments)
    {
        var builder = new StringBuilder();
        builder.Append("Ticket title: ").AppendLine(title);
        builder.Append("Ticket description: ").Append(description);
        if (comments.Count > 0)
        {
            builder.AppendLine().AppendLine().AppendLine("Internal comments (most recent last):");
            foreach (var comment in comments)
                builder.Append("- ").AppendLine(comment);
        }
        return builder.ToString();
    }

    public async Task<AiAnalysisResult> AnalyzeAsync(string title, string description, IReadOnlyList<string> comments, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new ExternalServiceException($"{ProviderName} is not configured. Set {ApiKeyConfigPath} using user-secrets or environment variables.");

        var userPrompt = BuildUserPrompt(title, description, comments);
        try
        {
            using var response = await SendWithRetryAsync(userPrompt, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new ExternalServiceException($"{ProviderName} analysis failed with status {(int)response.StatusCode}.");

            using var document = JsonDocument.Parse(body);
            var content = ExtractContent(document);
            if (string.IsNullOrWhiteSpace(content))
                throw new ExternalServiceException($"{ProviderName} returned an empty analysis.");

            return JsonSerializer.Deserialize<AiAnalysisResult>(content, ResultJsonOptions)
                ?? throw new ExternalServiceException($"{ProviderName} returned an empty analysis.");
        }
        catch (ExternalServiceException) { throw; }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or KeyNotFoundException or InvalidOperationException)
        {
            throw new ExternalServiceException($"{ProviderName} returned an invalid or unavailable response.", ex);
        }
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(string userPrompt, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            HttpResponseMessage response;
            // A request message can only be sent once, so build a fresh one for each attempt.
            using var request = CreateRequest(userPrompt);
            try
            {
                response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            }
            catch (HttpRequestException) when (attempt < MaxAttempts)
            {
                await Task.Delay(RetryBackoff * attempt, cancellationToken);
                continue;
            }

            if (attempt < MaxAttempts && IsTransient(response.StatusCode))
            {
                response.Dispose();
                await Task.Delay(RetryBackoff * attempt, cancellationToken);
                continue;
            }

            return response;
        }

        return TooManyRequests;
    }

    private HttpResponseMessage TooManyRequests => new(HttpStatusCode.TooManyRequests)
    {
        Content = new StringContent($"The {Provider.ToString()} is currently overloaded. Please try again later.")
    };

    private static bool IsTransient(HttpStatusCode statusCode) => statusCode is
        HttpStatusCode.TooManyRequests or
        HttpStatusCode.InternalServerError or
        HttpStatusCode.BadGateway or
        HttpStatusCode.ServiceUnavailable or
        HttpStatusCode.GatewayTimeout;
}
