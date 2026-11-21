namespace CAR.Api.Services.Interfaces;

public interface ISqlSchemaService
{
    Task<string> GetSchemaContextAsync(
        CancellationToken cancellationToken = default);
}