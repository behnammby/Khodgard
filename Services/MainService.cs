namespace Khodgard.Services;

using Khodgard.Data;
using Khodgard.Exceptions;
using Khodgard.Models;
using Microsoft.EntityFrameworkCore;
using Timer = System.Timers.Timer;

public class MainService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MainService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        IConfiguration config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        ILogger<Map> logger = scope.ServiceProvider.GetRequiredService<ILogger<Map>>();
        UnitOfWork uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

        logger.LogInformation("Doing migration ...");
        try
        {
            AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();
        }
        catch (Exception exp)
        {
            logger.LogError(exp, "Migration failed");
        }

        int timerMinDelay = config.GetValue<int>("MainService:TimerMinDelay");

        Timer timer = new(timerMinDelay * 1_000);
        timer.Elapsed += (sender, e) =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                UnitOfWork uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

                Map map = uow.GetAMapToRun();

                Thread thread = new(async () => await map.ExecuteAsync(_scopeFactory, stoppingToken));

                thread.Name = $"Map#{map.Id}";
                thread.Start();
            }
            catch (NoMapsToRunException) { }
        };

        timer.Disposed += (sender, e) =>
        {
            using var scope = _scopeFactory.CreateScope();
            ILogger<Map> logger = scope.ServiceProvider.GetRequiredService<ILogger<Map>>();

            logger.LogInformation("MAIN service stopped");
        };

        logger.LogInformation("Clearing all locks");
        await uow.ClearAllLocksAsync();

        logger.LogInformation("MAIN servcie started");
        timer.Start();
    }
}