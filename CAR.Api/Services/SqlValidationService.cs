using Microsoft.SqlServer.TransactSql.ScriptDom;
using CAR.Api.Services.Interfaces;

namespace CAR.Api.Services;

public class SqlValidationService : ISqlValidationService
{
    private readonly HashSet<string> _allowedSchemas;

    public SqlValidationService(IConfiguration configuration)
    {
        var schemas = configuration
            .GetSection("CarSettings:AllowedSchemas")
            .Get<string[]>() ?? ["dbo"];

        _allowedSchemas = new HashSet<string>(
            schemas,
            StringComparer.OrdinalIgnoreCase);
    }

    public bool IsValid(string sql, out string error)
    {
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(sql))
        {
            error = "SQL query cannot be empty.";
            return false;
        }

        var parser = new TSql160Parser(initialQuotedIdentifiers: true);

        using var reader = new StringReader(sql);

        var fragment = parser.Parse(reader, out var errors);

        if (errors.Count > 0)
        {
            error = string.Join(
                "; ",
                errors.Select(e => e.Message));

            return false;
        }

        if (fragment is not TSqlScript script ||
            script.Batches.Count != 1 ||
            script.Batches[0].Statements.Count != 1)
        {
            error = "Only one SQL statement is allowed.";
            return false;
        }

        if (script.Batches[0].Statements[0] is not SelectStatement)
        {
            error = "Only SELECT statements are allowed.";
            return false;
        }

        var visitor = new SelectSafetyVisitor(_allowedSchemas);

        fragment.Accept(visitor);

        if (!string.IsNullOrWhiteSpace(visitor.Error))
        {
            error = visitor.Error;
            return false;
        }

        return true;
    }

    private sealed class SelectSafetyVisitor : TSqlFragmentVisitor
    {
        private readonly HashSet<string> _allowedSchemas;

        public string Error { get; private set; } = string.Empty;

        public SelectSafetyVisitor(HashSet<string> allowedSchemas)
        {
            _allowedSchemas = allowedSchemas;
        }

        //public override void Visit(QuerySpecification node)
        //{
        //    if (node.Into != null)
        //    {
        //        Error = "SELECT INTO is not allowed.";
        //    }

        //    base.Visit(node);
        //}

        public override void Visit(NamedTableReference node)
        {
            var name = node.SchemaObject;

            if (name.ServerIdentifier != null ||
                name.DatabaseIdentifier != null)
            {
                Error = "Cross-server and cross-database queries are not allowed.";
                return;
            }

            if (name.SchemaIdentifier != null)
            {
                var schemaName = name.SchemaIdentifier.Value;

                if (!_allowedSchemas.Contains(schemaName))
                {
                    Error = $"Schema '{schemaName}' is not allowed.";
                    return;
                }
            }

            if (name.BaseIdentifier.Value.StartsWith(
                    "#",
                    StringComparison.Ordinal))
            {
                Error = "Temporary tables are not allowed.";
                return;
            }

            base.Visit(node);
        }
    }
}