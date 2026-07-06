using Api.Repositories.Interfaces;
using Api.Services;
using Microsoft.Extensions.Options;

namespace Api.BackgroundServices;

/// <summary>
/// Runs on a configurable interval and removes all messages that have passed their
/// expiration date. Uses a scoped service factory because <see cref="IMessageRepository"/>
/// is registered as Scoped, while BackgroundService is Singleton.
/// </summary>
public sealed class ExpiredMessageCleanupService(
    IServiceScopeFactory scopeFactory,
    IOptions<AppOptions> appOptions,
    ILogger<ExpiredMessageCleanupService> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(appOptions.Value.CleanupIntervalMinutes);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Expired message cleanup service started. Interval: {Interval} minutes.",
            _interval.TotalMinutes);

        try
        {
            // Short delay before the first run so the app is fully initialized
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await RunCleanupAsync(stoppingToken);
                await Task.Delay(_interval, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown — host is stopping, no action needed
            logger.LogInformation("Expired message cleanup service is stopping.");
        }
    }

    private async Task RunCleanupAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();

            var deleted = await repository.DeleteExpiredAsync(ct);

            if (deleted > 0)
                logger.LogInformation("Cleanup: removed {Count} expired message(s).", deleted);
            else
                logger.LogDebug("Cleanup: no expired messages found.");
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during expired message cleanup.");
        }
    }
}
