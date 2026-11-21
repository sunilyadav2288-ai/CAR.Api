# CAR SQL Assistant API

A .NET 10 Web API that accepts natural-language questions, uses Ollama to generate SQL, validates the generated query, and executes it against SQL Server.

## Requirements

- .NET 10 SDK
- SQL Server
- Ollama
- The Ollama model configured in `CAR.Api/appsettings.json` (default: `qwen2.5-coder:1.5b`)

## Setup

1. Create the database and sample data by running `CAR.Api/Database/CreateDatabase.sql` in SQL Server Management Studio or Azure Data Studio.
2. Update `CAR.Api/appsettings.json` with the SQL Server connection string and Ollama settings.
3. Start Ollama and pull the configured model:

   ```powershell
   ollama pull qwen2.5-coder:1.5b
   ```

4. Run the API from the repository directory:

   ```powershell
   dotnet run --project .\CAR.Api\CAR.Api.csproj
   ```

## Configuration

The main settings are stored in `CAR.Api/appsettings.json`:

- `ConnectionStrings:DefaultConnection` - SQL Server connection string.
- `Ollama:BaseUrl` - Ollama service URL.
- `Ollama:Model` - Ollama model used for SQL generation.
- `CarSettings:AllowedSchemas` - SQL schemas that may be queried.
- `CarSettings:MaxRows` - Maximum number of returned rows.

For local secrets or environment-specific values, use user secrets or `appsettings.Development.json` rather than committing sensitive values.

## Swagger

When running in the Development environment, Swagger UI is available at:

`https://localhost:<port>/swagger`

## API endpoint

### Ask a question

`POST /api/car/ask`

Example request:

```json
{
  "question": "Which employees work in the IT department?"
}
```

The API returns the generated SQL result or an error response when the request cannot be validated or executed.

## Project structure

- `CAR.Api/Controllers` - HTTP API controllers.
- `CAR.Api/Services` - Ollama, SQL generation, validation, schema, and execution services.
- `CAR.Api/Models` - Request and response models.
- `CAR.Api/Database/CreateDatabase.sql` - Database schema and sample data.
