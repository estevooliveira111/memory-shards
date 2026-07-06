using Api.Extensions;
using Api.Middleware;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using System.Reflection;

// ─── Serilog bootstrap logger ────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    // ── Load .env into environment variables BEFORE builder reads configuration ─
    // DotNetEnv maps A__B=value to configuration key A:B automatically
    DotNetEnv.Env.Load();

    var builder = WebApplication.CreateBuilder(args);

    // Make env vars (including those from .env) available to IConfiguration
    builder.Configuration.AddEnvironmentVariables();

    // ── Serilog ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

    // ── Controllers & CORS ───────────────────────────────────────────────────
    builder.Services.AddControllers();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
            if (allowedOrigins != null && allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
            }
            else
            {
                policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:5175").AllowAnyMethod().AllowAnyHeader();
                // Alternatively, in production, we might allow any origin if public API is intended:
                // policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            }
        });
    });

    // ── Swagger / OpenAPI ─────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title       = "Memory Shards API",
            Version     = "v1",
            Description = "Secure temporary message sharing with optional AES-256 encryption."
        });

        // Include XML comments for Swagger descriptions
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    });

    // ── Application services (EF, repos, services, background, validators) ───
    builder.Services.AddApplicationServices(builder.Configuration);

    // ─────────────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Global exception handler (must be first in pipeline) ─────────────────
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // ── Swagger JSON (keeps OpenAPI spec available for tooling) ────────────────────
    app.UseSwagger(options => options.RouteTemplate = "openapi/{documentName}.json");

    // ── Scalar UI (modern API reference at /scalar) ─────────────────────────
    app.MapScalarApiReference(options =>
    {
        options.Title           = "Memory Shards API";
        options.Theme           = ScalarTheme.DeepSpace;
        options.DefaultHttpClient = new(ScalarTarget.Http, ScalarClient.HttpClient);
        options.OpenApiRoutePattern = "/openapi/{documentName}.json";
    });

    // ── HTTPS redirect ────────────────────────────────────────────────────────
    app.UseHttpsRedirection();

    // ── CORS & Routing ────────────────────────────────────────────────────────
    app.UseCors("AllowFrontend");
    
    // ── Static Files (React Front-end) ────────────────────────────────────────
    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseRouting();
    app.MapControllers();
    app.MapFallbackToFile("index.html");

    // ── Apply EF migrations automatically ────────────────────────────────────
    await app.ApplyMigrationsAsync();

    Log.Information("Memory Shards API starting on {Urls}", string.Join(", ", builder.WebHost.GetSetting("urls") ?? "default ports"));

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
