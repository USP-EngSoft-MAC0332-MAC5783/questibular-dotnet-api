namespace Portfolio.DataAccess.Postgres;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal sealed class DatabaseMigrationHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<DatabaseMigrationHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();

        try
        {
            await context.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("PostgreSQL database migrations applied successfully.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to apply PostgreSQL database migrations.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
