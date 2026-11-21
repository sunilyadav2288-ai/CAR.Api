using System.Text.Json;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using CAR.Api.Models;
using CAR.Api.Services.Interfaces;

namespace CAR.Api.Services;

public class SqlExecutionService : ISqlExecutionService
{
    private readonly IConfiguration _configuration;

    public SqlExecutionService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<List<Dictionary<string, object?>>> ExecuteAsync(
        string sql,
        IEnumerable<SqlParameterModel> parameters,
        CancellationToken cancellationToken = default)
    {
        var connectionString =
            _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "SQL Server connection string is missing.");

        var commandTimeout = _configuration.GetValue<int>(
            "CarSettings:CommandTimeoutSeconds", 15);

        var maxRows = _configuration.GetValue<int>(
            "CarSettings:MaxRows", 100);

        var dynamicParameters = new DynamicParameters();

        foreach (var parameter in parameters)
        {
            if (!Regex.IsMatch(
                    parameter.Name,
                    @"^@[A-Za-z_][A-Za-z0-9_]*$"))
            {
                throw new InvalidOperationException(
                    $"Invalid SQL parameter name: {parameter.Name}");
            }

            var value = ConvertJsonValue(parameter.Value);
            var dbType = GetDbType(parameter.DataType);

            dynamicParameters.Add(
                parameter.Name,
                value,
                dbType);
        }

        await using var connection = new SqlConnection(connectionString);

        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            dynamicParameters,
            commandTimeout: commandTimeout,
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync(command);

        // Limit the number of rows returned by the API.
        // For large production queries, also enforce a SQL-side row limit.
        return rows
            .Take(maxRows)
            .Select(row =>
            {
                var dictionary = (IDictionary<string, object>)row;

                return dictionary.ToDictionary(
                    item => item.Key,
                    item => item.Value is DBNull ? null : item.Value);
            })
            .ToList();
    }

    private static object? ConvertJsonValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String =>
                value.TryGetDateTime(out var date)
                    ? date
                    : value.GetString(),

            JsonValueKind.Number =>
                value.TryGetInt32(out var intValue)
                    ? intValue
                    : value.TryGetInt64(out var longValue)
                        ? longValue
                        : value.GetDecimal(),

            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,

            _ => throw new InvalidOperationException(
                "Unsupported SQL parameter value.")
        };
    }

    private static DbType GetDbType(string dataType)
    {
        return dataType.ToLowerInvariant() switch
        {
            "int" => DbType.Int32,
            "bigint" => DbType.Int64,
            "smallint" => DbType.Int16,
            "bit" => DbType.Boolean,
            "decimal" => DbType.Decimal,
            "numeric" => DbType.Decimal,
            "float" => DbType.Double,
            "real" => DbType.Single,
            "date" => DbType.Date,
            "datetime" => DbType.DateTime,
            "datetime2" => DbType.DateTime2,
            "uniqueidentifier" => DbType.Guid,
            "varchar" => DbType.AnsiString,
            "char" => DbType.AnsiStringFixedLength,
            "nvarchar" => DbType.String,
            "nchar" => DbType.StringFixedLength,
            _ => throw new InvalidOperationException(
                $"Unsupported SQL parameter type: {dataType}")
        };
    }
}