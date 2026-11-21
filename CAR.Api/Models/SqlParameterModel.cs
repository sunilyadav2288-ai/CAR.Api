using System.Text.Json;

namespace CAR.Api.Models;

public class SqlParameterModel
{
    public string Name { get; set; } = string.Empty;

    public string DataType { get; set; } = string.Empty;

    public JsonElement Value { get; set; }
}