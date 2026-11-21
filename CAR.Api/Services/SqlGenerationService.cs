using System.Text.Json;
using CAR.Api.Models;
using CAR.Api.Services.Interfaces;

namespace CAR.Api.Services;

public class SqlGenerationService : ISqlGenerationService
{
    private readonly IOllamaService _ollamaService;

    public SqlGenerationService(IOllamaService ollamaService)
    {
        _ollamaService = ollamaService;
    }

    public async Task<SqlGenerationResult> GenerateSqlAsync(
        string question,
        string schemaContext,
        CancellationToken cancellationToken = default)
    {
        var prompt = $$"""
            You are an expert Microsoft SQL Server query generator.

            Generate a SQL Server SELECT query based on the user's question
            and the database schema provided below.

            DATABASE SCHEMA:
            {{schemaContext}}

            USER QUESTION:
            {{question}}

            RULES:
            1. Generate SQL dynamically using only the supplied schema.
            2. Generate only one read-only SELECT statement.
            3. Never generate INSERT, UPDATE, DELETE, DROP, ALTER,
               CREATE, TRUNCATE, EXEC or stored procedure calls.
            4. Never generate SELECT INTO.
            5. Use SQL Server syntax.
            6. Use table aliases for joins.
            7. Use parameterized SQL for user-provided values.
            8. Do not invent table names or column names.
            9. Return only valid JSON. Do not include Markdown.

            RESPONSE FORMAT:
            {
              "sql": "SELECT TOP (10) ... WHERE DepartmentId = @DepartmentId",
              "parameters": [
                {
                  "name": "@DepartmentId",
                  "dataType": "int",
                  "value": 1
                }
              ]
            }
            """;

        var response = await _ollamaService.GenerateAsync(
            prompt,
            cancellationToken);

        var json = ExtractJson(response);

        var result = JsonSerializer.Deserialize<SqlGenerationResult>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (result == null || string.IsNullOrWhiteSpace(result.Sql))
        {
            throw new InvalidOperationException(
                "Ollama could not generate a valid SQL query.");
        }

        return result;
    }

    private static string ExtractJson(string response)
    {
        var json = response.Trim();

        if (json.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewLine = json.IndexOf('\n');

            if (firstNewLine >= 0)
                json = json[(firstNewLine + 1)..];

            var lastFence = json.LastIndexOf("```",
                StringComparison.Ordinal);

            if (lastFence >= 0)
                json = json[..lastFence];
        }

        json = json.Trim();

        var start = json.IndexOf('{');
        var end = json.LastIndexOf('}');

        if (start < 0 || end <= start)
        {
            throw new InvalidOperationException(
                "Ollama response does not contain valid JSON.");
        }

        return json[start..(end + 1)];
    }
}