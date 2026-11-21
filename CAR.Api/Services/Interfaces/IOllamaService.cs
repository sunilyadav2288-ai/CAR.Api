namespace CAR.Api.Services.Interfaces;

public interface IOllamaService
{
    Task<string> GenerateAsync(
        string prompt,
        CancellationToken cancellationToken = default);
}