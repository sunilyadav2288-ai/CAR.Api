using CAR.Api.Models;

namespace CAR.Api.Services.Interfaces;

public interface ISqlGenerationService
{
    Task<SqlGenerationResult> GenerateSqlAsync(
        string question,
        string schemaContext,
        CancellationToken cancellationToken = default);
}