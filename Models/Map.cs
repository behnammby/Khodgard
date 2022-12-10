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
    public int DepthLimit { get; set; }
    public int SyncSegments { get; set; }
    public int SyncMultifold { get; set; }
    public int SyncDelay { get; set; }
    public int ClearAgeDelay { get; set; }
    public int ClearCountDelay { get; set; }
    public int MaxAge { get; set; }
    public int MaxLines { get; set; }
    public int MaxDeleteLines { get; set; }
    public bool IsRunning { get; set; }
    public bool LockedSync { get; set; }
    public bool LockedClearAge { get; set; }
    public bool LockedClearCount { get; set; }
    public bool Enabled { get; set; }

    public async Task ExecuteAsync(IServiceScopeFactory scopeFactory, CancellationToken stoppingToken)
    {
        LockMapType[] lockTypes = new[] { LockMapType.Sync, LockMapType.ClearAge, LockMapType.ClearCount };
        // LockMapType[] lockTypes = new[] { LockMapType.Sync, LockMapType.ClearByAge };
        // LockMapType[] lockTypes = new[] { LockMapType.Sync };
        // LockMapType[] lockTypes = new[] { LockMapType.ClearByCount };

        using var scope = scopeFactory.CreateScope();

        ILogger<Map> logger = scope.ServiceProvider.GetRequiredService<ILogger<Map>>();
        UnitOfWork uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

        if (!await uow.MapsRepo.MarkMapAsRunning(this))
        {
            logger.LogError("Map#{Id}: Couldn't mark the map, skipping.", Id);
            return;
        }

        List<Timer> timers = new();

        foreach (var lockType in lockTypes)
        {
            int minDelay = lockType switch
            {
                LockMapType.Sync => SyncDelay * 1_000,
                LockMapType.ClearAge => ClearAgeDelay * 1_000,
                LockMapType.ClearCount => ClearCountDelay * 1_000,

                _ => throw new LockMapTypeInvalidException()
            };

            Timer timer = new(minDelay);
            timers.Add(timer);

            timer.Elapsed += async (sender, e) =>
            {
                using var scope = scopeFactory.CreateScope();

                ILogger<Map> logger = scope.ServiceProvider.GetRequiredService<ILogger<Map>>();
                UnitOfWork uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

                logger.LogTrace("Map#{Id}: Timer {LockType} elapsed", Id, lockType);

                if (stoppingToken.IsCancellationRequested)
                {
                    logger.LogTrace("Map#{Id}: Cancellation requested, Stopping the timer {TimerType}.", Id, lockType);
                    timer.Stop();
                    timer.Close();
                    timer.Dispose();
                    return;
                }

                try
                {
                    logger.LogTrace("Map#{Id}: Trying to lock {LockType}", Id, lockType);
                    if (!await uow.MapsRepo.LockMap(this, lockType))
                    {
                        logger.LogTrace("Map#{Id}: Map is {LockType} locked by another process, skiping.", Id, lockType);
                        return;
                    }

                    MapBehavior behavior = new(Id, logger, uow, scopeFactory);

                    var handler = lockType switch
                    {
                        LockMapType.Sync => behavior.SynchronizeLinesAsync(stoppingToken, timers),
                        LockMapType.ClearAge => behavior.ClearLinesByAgeAsync(MaxAge, MaxDeleteLines, stoppingToken, timers),
                        LockMapType.ClearCount => behavior.ClearLinesByCountAsync(MaxLines, MaxDeleteLines, stoppingToken, timers),

                        _ => throw new LockMapTypeInvalidException()
                    };


                    await handler;

                    bool unlock = await uow.MapsRepo.UnlockMap(this, lockType);
                    return;
                }
                catch (Exception exp)
                {
                    logger.LogError("Map#{Id}: Error occured, Stopping the timer {LockType}.", Id, lockType);
                    logger.LogError("Map#{Id}: {Error} {InnerError}", Id, exp.GetType().Name + ": " + exp.Message, exp?.InnerException is not null ? exp.InnerException.GetType().Name + " " + exp?.InnerException?.Message : string.Empty);
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
                    bool result = await uow.MapsRepo.ReleaseMap(this);
                    if (result)
                        logger.LogInformation("Map#{Id}: Map released", Id);
                    else
                        logger.LogError("Map#{Id}: Couldn't release the map!", Id);
                }
            };

            logger.LogInformation("Map#{Id}: Timer {LockType} started", Id, lockType);
            timer.Start();
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Map map)
            return false;

        return Id == map.Id;
    }

    public override int GetHashCode() => Id;
    public static bool operator ==(Map? a, Map? b)
    {
        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }
    public static bool operator !=(Map? a, Map? b)
    {
        if (a is null || b is null)
            return false;

        return !a.Equals(b);
    }
}