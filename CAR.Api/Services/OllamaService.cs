using System.Net.Http.Json;
using System.Text.Json;
using CAR.Api.Models;
using CAR.Api.Services.Interfaces;

namespace CAR.Api.Services;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OllamaService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> GenerateAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var model = _configuration["Ollama:Model"]
                    ?? "llama3.2:3b";

        var request = new
        {
            model,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            stream = false
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "api/chat",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(
            cancellationToken: cancellationToken);

        return result?.Message?.Content
               ?? throw new InvalidOperationException(
                   "Ollama returned an empty response.");
    }
}