namespace CAR.Api.Models;

public class SqlGenerationResult
{
    public string Sql { get; set; } = string.Empty;

    public List<SqlParameterModel> Parameters { get; set; } = [];
}