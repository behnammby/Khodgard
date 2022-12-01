using Khodgard.Data;
using Khodgard.Enumerations;
using Khodgard.Exceptions;
using Timer = System.Timers.Timer;


namespace Khodgard.Models;

public class Map
{
    public Map()
    {
        Name = string.Empty;

        Source = new();
        Target = new();
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public MapType MapType { get; set; }
    public Market Source { get; set; }
    public Market Target { get; set; }
    public double Ratio { get; set; }
    public int SyncMinDelay { get; set; }
    public int ClearByAgeMinDelay { get; set; }
    public int ClearByCountMinDelay { get; set; }
    public int MaxAge { get; set; }
    public int MaxLines { get; set; }
    public bool IsRunning { get; set; }
    public bool LockedForSync { get; set; }
    public bool LockedForClearByAge { get; set; }
    public bool LockedForClearByCount { get; set; }
    public bool Enabled { get; set; }

    public async Task ExecuteAsync(IServiceScopeFactory scopeFactory, CancellationToken stoppingToken)
    {
        // LockMapType[] lockTypes = new[] { LockMapType.Sync, LockMapType.ClearByAge, LockMapType.ClearByCount };
        LockMapType[] lockTypes = new[] { LockMapType.Sync, LockMapType.ClearByAge };

        using var scope = scopeFactory.CreateScope();

        ILogger<Map> logger = scope.ServiceProvider.GetRequiredService<ILogger<Map>>();
        UnitOfWork uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

        if (!await uow.MarkMapAsRunning(this))
        {
            logger.LogInformation("Map#{Id}: Couldn't mark the map, skipping.", Id);
            return;
        }

        List<Timer> timers = new();

        foreach (var lockType in lockTypes)
        {
            int minDelay = lockType switch
            {
                LockMapType.Sync => SyncMinDelay * 1_000,
                LockMapType.ClearByAge => ClearByAgeMinDelay * 1_000,
                LockMapType.ClearByCount => ClearByCountMinDelay * 1_000,

                _ => throw new LockMapTypeInvalidException()
            };

            Timer timer = new(minDelay);
            timers.Add(timer);

            timer.Elapsed += async (sender, e) =>
            {
                using var scope = scopeFactory.CreateScope();

                ILogger<Map> logger = scope.ServiceProvider.GetRequiredService<ILogger<Map>>();
                UnitOfWork uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

                if (stoppingToken.IsCancellationRequested)
                {
                    logger.LogInformation("Map#{Id}: Cancellation requested, Stopping the timer {TimerType}.", Id, lockType);
                    timer.Stop();
                    timer.Close();
                    timer.Dispose();
                    return;
                }

                try
                {
                    if (!await uow.LockMap(this, lockType))
                    {
                        logger.LogInformation("Map#{Id}: Map is {LockType} locked by another process, skiping.", Id, lockType);
                        return;
                    }

                    MapBehavior behavior = new(Id, logger, uow);

                    var handler = lockType switch
                    {
                        LockMapType.Sync => behavior.SynchronizeLinesAsync(stoppingToken, timers),
                        LockMapType.ClearByAge => behavior.ClearLinesByAgeAsync(MaxAge, stoppingToken, timers),
                        LockMapType.ClearByCount => behavior.ClearLinesByCountAsync(MaxLines, stoppingToken, timers),

                        _ => throw new LockMapTypeInvalidException()
                    };

                    await handler;

                    bool unlock = await uow.UnlockMap(this, lockType);
                    return;
                }
                catch (Exception exp)
                {
                    logger.LogInformation("Map#{Id}: Error occured, Stopping the timer {LockType}.", Id, lockType);
                    logger.LogInformation("Map#{Id}: {Error}", Id, exp.Message);
                    timer.Stop();
                    timer.Close();
                    timer.Dispose();
                    return;
                }
            };

            timer.Disposed += async (sender, e) =>
            {
                using var scope = scopeFactory.CreateScope();

                ILogger<Map> logger = scope.ServiceProvider.GetRequiredService<ILogger<Map>>();

                logger.LogInformation("Map#{Id}: Timer {LockType} disposed", Id, lockType);

                if (!timers.Any(_ => _.Enabled))
                {
                    UnitOfWork uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

                    logger.LogInformation("Map#{Id}: All timers are stopped! Releasing the map now", Id);
                    bool result = await uow.ReleaseMap(this);
                    if (result)
                        logger.LogInformation("Map#{Id}: Map released", Id);
                    else
                        logger.LogInformation("Map#{Id}: Couldn't release the map!", Id);
                }
            };

            logger.LogInformation("Map#{Id}: Timer {LockType} started", Id, lockType);
            timer.Start();
        }
    }
}