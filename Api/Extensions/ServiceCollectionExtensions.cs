using Api.BackgroundServices;
using Api.Infrastructure.Data;
using Api.Repositories;
using Api.Repositories.Interfaces;
using Api.Services;
using Api.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Api.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers all application services, repositories, and infrastructure.</summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration binding
        services.Configure<AppOptions>(configuration.GetSection(AppOptions.SectionName));

        // EF Core + SQLite
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        // AutoMapper
        services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);

        // FluentValidation — register all validators in the assembly
        services.AddValidatorsFromAssemblyContaining<MessageRepository>();

        // Repository pattern
        services.AddScoped<IMessageRepository, MessageRepository>();

        // Domain services
        services.AddScoped<IMessageService, MessageService>();
        services.AddSingleton<IEncryptionService, EncryptionService>();

        // Background service
        services.AddHostedService<ExpiredMessageCleanupService>();

        return services;
    }

    /// <summary>Applies any pending EF Core migrations on startup.</summary>
    public static async Task ApplyMigrationsAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}
