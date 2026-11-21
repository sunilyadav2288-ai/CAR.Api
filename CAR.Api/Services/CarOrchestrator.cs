using CAR.Api.Models;
using CAR.Api.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;

namespace CAR.Api.Services;

public class CarOrchestrator : ICarOrchestrator
{
    private readonly ISqlSchemaService _schemaService;
    private readonly ISqlGenerationService _generationService;
    private readonly ISqlValidationService _validationService;
    private readonly ISqlExecutionService _executionService;
    private readonly IOllamaService _ollamaService;
    private readonly ILogger<CarOrchestrator> _logger;

    public CarOrchestrator(
        ISqlSchemaService schemaService,
        ISqlGenerationService generationService,
        ISqlValidationService validationService,
        ISqlExecutionService executionService,
        IOllamaService ollamaService,
        ILogger<CarOrchestrator> logger)
    {
        _schemaService = schemaService;
        _generationService = generationService;
        _validationService = validationService;
        _executionService = executionService;
        _ollamaService = ollamaService;
        _logger = logger;
    }

    public async Task<CarResponse> ProcessAsync(
        CarRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            throw new ArgumentException(
                "Please provide a question.");
        }

        _logger.LogInformation(
            "Processing CAR question: {Question}",
            request.Question);

        // Step 1: Discover SQL Server schema dynamically.
        var schemaContext =
            await _schemaService.GetSchemaContextAsync(
                cancellationToken);

        // Step 2: Generate SQL using Ollama.
        var sqlResult =
            await _generationService.GenerateSqlAsync(
                request.Question,
                schemaContext,
                cancellationToken);

        // Step 3: Validate generated SQL.
        if (!_validationService.IsValid(
                sqlResult.Sql,
                out var validationError))
        {
            _logger.LogWarning(
                "Generated SQL was rejected: {Error}",
                validationError);

            throw new InvalidOperationException(
                $"Generated SQL is not allowed: {validationError}");
        }

        // Step 4: Execute validated SQL.
        var data = await _executionService.ExecuteAsync(
            sqlResult.Sql,
            sqlResult.Parameters,
            cancellationToken);

        // Step 5: Ask Ollama to explain the database results.
        var dataJson = JsonSerializer.Serialize(data);

        var answerPrompt = $$"""
            You are a helpful data assistant.

            USER QUESTION:
            {{request.Question}}

            DATABASE RESULTS:
            {{dataJson}}

            Explain the results in simple, clear language.

            Rules:
            1. Use only the supplied database results.
            2. Do not invent or assume information.
            3. If there are no records, clearly say no records were found.
            4. Keep the response concise.
            """;

        var answer = await _ollamaService.GenerateAsync(
            answerPrompt,
            cancellationToken);

        _logger.LogInformation(
            "CAR request completed. Rows returned: {RowCount}",
            data.Count);

        return new CarResponse
        {
            Question = request.Question,
            GeneratedSql = sqlResult.Sql,
            Data = data,
            Answer = answer
        };
    }
}