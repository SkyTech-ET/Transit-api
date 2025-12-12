using Transit.Domain.Data;
using Microsoft.Extensions.DependencyInjection;

public class LogCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan CleanupInterval = TimeSpan.FromDays(1); // Run every day
    private readonly TimeSpan CutoffInterval = TimeSpan.FromDays(90); // Check every 90 days

    public LogCleanupService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DateTime lastCleanupTime = DateTime.UtcNow;

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (DateTime.UtcNow - lastCleanupTime >= CutoffInterval)
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            var cutoffDate = DateTime.UtcNow.AddMonths(-3);

                            // dbContext.ActionLog.RemoveRange(dbContext.ActionLog.Where(log => log.Date < cutoffDate));
                            await dbContext.SaveChangesAsync(stoppingToken);
                            lastCleanupTime = DateTime.UtcNow;
                        }
                    }

                    await Task.Delay(CleanupInterval, stoppingToken); // Check daily
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    // Log the exception but continue running
                    Console.WriteLine($"LogCleanupService error: {ex.Message}");
                    // Wait a bit before retrying to avoid rapid error loops
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the service is being stopped
        }
    }
}
