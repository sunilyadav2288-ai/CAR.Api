using System.Text.Json.Serialization;

namespace CAR.Api.Models;

public class OllamaResponse
{
    [JsonPropertyName("message")]
    public OllamaMessage? Message { get; set; }
}

public class OllamaMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}