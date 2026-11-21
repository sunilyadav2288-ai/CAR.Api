using CAR.Api.Services;
using CAR.Api.Services.Interfaces;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Swagger configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CAG SQL Assistant API",
        Version = "v1",
        Description = "Natural Language to SQL using Ollama and SQL Server"
    });
});

builder.Services.AddHttpClient<IOllamaService, OllamaService>(
    client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["Ollama:BaseUrl"]
            ?? "http://localhost:11434/");

        client.Timeout = TimeSpan.FromSeconds(
            builder.Configuration.GetValue<int>(
                "Ollama:TimeoutSeconds", 120));
    });

// SQL Server and CAR services
builder.Services.AddScoped<ISqlSchemaService, SqlSchemaService>();
builder.Services.AddScoped<ISqlGenerationService, SqlGenerationService>();
builder.Services.AddScoped<ISqlValidationService, SqlValidationService>();
builder.Services.AddScoped<ISqlExecutionService, SqlExecutionService>();

// Main orchestrator
builder.Services.AddScoped<ICarOrchestrator, CarOrchestrator>();


var app = builder.Build();
// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "CAG SQL Assistant API v1");

        options.RoutePrefix = "swagger";
    });
}
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
