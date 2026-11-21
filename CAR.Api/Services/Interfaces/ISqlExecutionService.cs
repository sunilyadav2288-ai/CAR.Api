namespace CAR.Api.Services.Interfaces;

public interface ISqlExecutionService
{
    Task<List<Dictionary<string, object?>>> ExecuteAsync(
        string sql,
        IEnumerable<CAR.Api.Models.SqlParameterModel> parameters,
        CancellationToken cancellationToken = default);
}