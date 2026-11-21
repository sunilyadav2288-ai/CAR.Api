namespace CAR.Api.Models;

public class CarResponse
{
    public string Question { get; set; } = string.Empty;

    public string GeneratedSql { get; set; } = string.Empty;

    public object? Data { get; set; }

    public string Answer { get; set; } = string.Empty;
}