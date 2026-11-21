using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;
using CAR.Api.Services.Interfaces;

namespace CAR.Api.Services;

public class SqlSchemaService : ISqlSchemaService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SqlSchemaService> _logger;

    public SqlSchemaService(
        IConfiguration configuration,
        ILogger<SqlSchemaService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GetSchemaContextAsync(
        CancellationToken cancellationToken = default)
    {
        var connectionString =
            _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "SQL Server connection string is missing.");

        var allowedSchemas =
            _configuration.GetSection("CarSettings:AllowedSchemas")
                .Get<string[]>() ?? ["dbo"];

        const string sql = """
            SELECT
                s.name AS SchemaName,
                t.name AS TableName,
                c.name AS ColumnName,
                ty.name AS DataType,
                c.max_length AS MaxLength,
                c.is_nullable AS IsNullable,
                c.is_identity AS IsIdentity,
                CAST(
                    CASE WHEN pk.column_id IS NOT NULL THEN 1 ELSE 0 END
                    AS bit
                ) AS IsPrimaryKey
            FROM sys.tables t
            INNER JOIN sys.schemas s
                ON t.schema_id = s.schema_id
            INNER JOIN sys.columns c
                ON t.object_id = c.object_id
            INNER JOIN sys.types ty
                ON c.user_type_id = ty.user_type_id
            LEFT JOIN
            (
                SELECT
                    ic.object_id,
                    ic.column_id
                FROM sys.indexes i
                INNER JOIN sys.index_columns ic
                    ON i.object_id = ic.object_id
                    AND i.index_id = ic.index_id
                WHERE i.is_primary_key = 1
            ) pk
                ON c.object_id = pk.object_id
                AND c.column_id = pk.column_id
            WHERE s.name IN @AllowedSchemas
            ORDER BY s.name, t.name, c.column_id;

            SELECT
                ps.name AS ParentSchema,
                pt.name AS ParentTable,
                pc.name AS ParentColumn,
                rs.name AS ReferencedSchema,
                rt.name AS ReferencedTable,
                rc.name AS ReferencedColumn,
                fk.name AS ForeignKeyName
            FROM sys.foreign_keys fk
            INNER JOIN sys.foreign_key_columns fkc
                ON fk.object_id = fkc.constraint_object_id
            INNER JOIN sys.tables pt
                ON fkc.parent_object_id = pt.object_id
            INNER JOIN sys.schemas ps
                ON pt.schema_id = ps.schema_id
            INNER JOIN sys.columns pc
                ON pc.object_id = pt.object_id
                AND pc.column_id = fkc.parent_column_id
            INNER JOIN sys.tables rt
                ON fkc.referenced_object_id = rt.object_id
            INNER JOIN sys.schemas rs
                ON rt.schema_id = rs.schema_id
            INNER JOIN sys.columns rc
                ON rc.object_id = rt.object_id
                AND rc.column_id = fkc.referenced_column_id
            WHERE ps.name IN @AllowedSchemas
              AND rs.name IN @AllowedSchemas
            ORDER BY pt.name, fk.name;
            """;

        await using var connection = new SqlConnection(connectionString);

        await connection.OpenAsync(cancellationToken);

        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(
                sql,
                new { AllowedSchemas = allowedSchemas },
                commandTimeout: 15,
                cancellationToken: cancellationToken));

        var columns = (await multi.ReadAsync()).ToList();
        var relationships = (await multi.ReadAsync()).ToList();

        if (columns.Count == 0)
        {
            _logger.LogWarning(
                "No tables were discovered in the allowed schemas.");
        }

        var schema = new
        {
            Tables = columns,
            Relationships = relationships
        };

        return JsonSerializer.Serialize(
            schema,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
    }
}