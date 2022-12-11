using Khodgard.Data;
using Khodgard.Enumerations;
using Khodgard.Exceptions;
using Khodgard.Extensions;
using Timer = System.Timers.Timer;

namespace Khodgard.Models;

internal class MapBehavior
{
    private readonly Map _map;
    private readonly ILogger _logger;
    private readonly UnitOfWork _uow;
    private readonly IServiceScopeFactory _scopeFactory;

    public MapBehavior(int mapId, ILogger logger, UnitOfWork uow, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _uow = uow;
        _scopeFactory = scopeFactory;

        _map = _uow.MapsRepo.GetMapById(mapId);
        _map.Source.Exchange.Init();
        _map.Target.Exchange.Init();
    }

    private void LogCreateOrderException(Exception exp, decimal price, double amount, OrderSide side)
    {
        _logger.LogError("Creating order failed, Price= {Price}, Amount= {Amount}, Side= {side}", price, amount, side);
        _logger.LogError("Error: {Error} {Inner}", exp.Message, exp.InnerException?.Message ?? string.Empty);
    }
    private async Task RefreshLinesAsync(IEnumerable<Line> lines)
    {
        foreach (Line line in lines)
        {
            try
            {
                Line extistingLine = _uow.LinesRepo.GetLineByPrice(_map, line.Price);
                extistingLine.Amount = line.Amount;
                extistingLine.Updated = DateTime.UtcNow;
                _uow.LinesRepo.Update(extistingLine);
            }
            catch (LineNotFoundException)
            {
                Line newLine = new(line.Price, line.Amount, line.Side, _map);
                _uow.LinesRepo.Create(newLine);
            }
        }

        await _uow.CommitAsync();
    }
    private Line? GetLine(decimal price)
    {
        try
        {
            return _uow.LinesRepo.GetLineByPrice(_map, price);
        }
        catch (LineNotFoundException)
        {
            return null;
        }
    }
    private async Task CreateLineAsync(decimal price, double amount, OrderSide side)
    {
        Line line = new(price, amount, side, _map);
        Order order = new(price, amount, side, _map.Target, line);

        try
        {
            await _map.Target.Exchange.CreateOrderAsync(order, _map.Target.PricePrecision, _map.Target.AmountPrecision);
            _uow.LinesRepo.Create(line);
            _uow.OrdersRepo.Create(order);
        }
        catch (CreateOrderException exp)
        {
            LogCreateOrderException(exp, price, amount, side);
            return;
        }

        await _uow.CommitAsync();
    }
    private async Task UpdateLineAsync(Line line, double amount)
    {
        if (amount > line.Amount)
        {
            double diff = amount - line.Amount;
            Order order = new(price: line.Price, amount: diff, side: line.Side, market: _map.Target, line);

            try
            {
                await _map.Target.Exchange.CreateOrderAsync(order, _map.Target.PricePrecision, _map.Target.AmountPrecision);
                line.Amount += diff;
                line.Updated = DateTime.UtcNow;
                _uow.LinesRepo.Update(line);
                _uow.OrdersRepo.Create(order);
            }
            catch (CreateOrderException exp)
            {
                LogCreateOrderException(exp, line.Price, amount, line.Side);
                return;
            }
        }
        else if (amount < line.Amount)
        {
            var lineOrders = _uow.OrdersRepo.GetOrdersOfLine(line);
            double diff = line.Amount - amount;

            if (diff <= 0)
                return;

            double deleteAmount = 0;
            Order order = new(price: line.Price, amount: diff, side: line.Side, market: _map.Target, line: line);

            try
            {
                await _map.Target.Exchange.CreateOrderAsync(order, _map.Target.PricePrecision, _map.Target.AmountPrecision);
                line.Amount += amount;
                foreach (Order lineOrder in lineOrders)
                {
                    bool result = await _map.Target.Exchange.CancelOrderAsync(lineOrder);
                    if (result)
                    {
                        _uow.OrdersRepo.Delete(lineOrder);
                        deleteAmount += lineOrder.Amount;
                    }
                }
            }
            catch (CreateOrderException exp)
            {
                LogCreateOrderException(exp, line.Price, amount, line.Side);
                return;
            }

            line.Amount -= deleteAmount;
            if (line.Amount > amount * 1.5)
                if (await _map.Target.Exchange.CancelOrderAsync(order))
                    line.Amount -= amount;

            line.Updated = DateTime.UtcNow;
            _uow.LinesRepo.Update(line);
            _uow.OrdersRepo.Create(order);
        }

        await _uow.CommitAsync();
    }
    private async Task DeleteLineAsync(Line line)
    {
        LogTrace("Deleting line {Id}, pirce= {Price}, amount= {Amount}", line.Id, line.Price, line.Amount);
        var orders = _uow.OrdersRepo.GetOrdersOfLine(line);
        foreach (Order order in orders)
        {
            bool result = await _map.Target.Exchange.CancelOrderAsync(order);
            if (!result)
                LogError("Couldn't delete the order Id={Order}, price= {Pirce}, amount= {Amount}", order.Uid, order.Price, order.Amount);
            _uow.OrdersRepo.Delete(order);
        }

        _uow.LinesRepo.Delete(line);

        await _uow.CommitAsync();
        LogTrace("Deleted");
    }
    private IEnumerable<Line> GetLinesByAge(Map map, int maxAge, int maxDeleteLines) => _uow.LinesRepo.GetLinesByAge(map, maxAge, maxDeleteLines);
    private IEnumerable<Line> GetLinesByCount(Map map, int maxLines, int maxDeleteLines) => _uow.LinesRepo.GetLinesByCount(map, maxLines, maxDeleteLines);
    private void Log(string msg, params object?[] args)
    {
        msg = $"Map#{_map.Id}: " + msg;
        _logger.LogInformation(msg, args);
    }
    private void LogTrace(string msg, params object?[] args)
    {
        msg = $"Map#{_map.Id}: " + msg;
        _logger.LogTrace(msg, args);
    }
    private void LogError(string msg, params object?[] args)
    {
        msg = $"Map#{_map.Id}: " + msg;
        _logger.LogTrace(msg, args);
    }
    private void CheckTimers(IEnumerable<Timer> timers)
    {
        if (timers.Any(_ => !_.Enabled))
            throw new TimerStoppedException();
    }
    private IEnumerable<IEnumerable<Line>> DivideIntoSegments(IEnumerable<Line> shuffled, int denom)
    {
        int length = shuffled.Count() / denom;
        List<IEnumerable<Line>> segments = new();
        for (int i = 0; i < denom; i++)
        {
            if (i == denom - 1)
                segments.Add(shuffled.TakeLast(shuffled.Count() - i * length));
            else
                segments.Add(shuffled.Skip(i * length).Take(length));
        }

        return segments;
    }
    private async Task SynchronizeLineSegment(IEnumerable<Line>? segment, CancellationToken stoppingToken)
    {
        if (segment is null)
            return;

        int i = 0;
        Log("Syncying line segmegment, Count= {Count}", segment.Count());
        foreach (Line sourceLine in segment)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            LogTrace("Syncing line Price= {Line}, Amount= {Amount}, Side= {Side}", sourceLine.Price, sourceLine.Amount, sourceLine.Side);
            sourceLine.ApplyRatio(_map.Ratio);
            Line? targetLine = GetLine(sourceLine.Price);

            if (targetLine is not null)
            {
                if (sourceLine.Amount == 0)
                    await DeleteLineAsync(targetLine);
                else
                    await UpdateLineAsync(targetLine, sourceLine.Amount);
            }
            else
            {
                if (sourceLine.Amount > 0)
                    await CreateLineAsync(sourceLine.Price, sourceLine.Amount, sourceLine.Side);
            }
            LogTrace("Syncying finished");
            i++;
            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }
        Log("Synchronizing segment finished, {Synced} out of {Lines} synced.", i, segment.Count());
    }

    public async Task SynchronizeLinesAsync(CancellationToken stoppingToken, IEnumerable<Timer> timers)
    {
        if (stoppingToken.IsCancellationRequested)
            return;

        Log("Synchronizing lines started");

        IEnumerable<Line> sourceLines = await _map.Source.Exchange.GetDepthAsync(_map.Source, _map.DepthLimit * _map.SyncMultifold, _map);
        Log("{Count} lines got from source, sychronizing", sourceLines.Count());

        var shuffled = sourceLines.ToList().Shuffle().Take(_map.DepthLimit);
        var segments = DivideIntoSegments(shuffled, _map.SyncSegments);

        Log("{Count} lines candidates for synchronizing, in {Segments} segments", shuffled.Count(), segments.Count());

        // List<Thread> threads = new();
        foreach (var segment in segments)
        {
            if (segment is null)
                continue;

            await SynchronizeLineSegment(segment, stoppingToken);
            await Task.Delay(TimeSpan.FromMilliseconds(500));

            // Thread thread = new(async () =>
            // {
            //     using var scope = _scopeFactory.CreateScope();
            //     var logger = scope.ServiceProvider.GetRequiredService<ILogger<Map>>();
            //     var uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            //     MapBehavior behavior = new(mapId: _map.Id, logger, uow, _scopeFactory);
            //     await behavior.SynchronizeLineSegment(segment, stoppingToken);
            // });

            // thread.Name = $"Segment, Count= {segment.Count()}";
            // threads.Add(thread);
        }

        // foreach (var thread in threads)
        //     thread.Start();

        // while (threads.Any(_ => _.ThreadState != ThreadState.Stopped)) { }

        // Log("Synchronizying lines finished using {Count} threads", threads.Count);
        Log("Synchronizying lines finished using {Count} segments", segments.Count());

        CheckTimers(timers);
    }
    public async Task ClearLinesByAgeAsync(int maxAge, int maxDeleteLines, CancellationToken stoppingToken, IEnumerable<Timer> timers)
    {
        Log("Clear lines by age started");

        IEnumerable<Line> lines = GetLinesByAge(_map, maxAge, maxDeleteLines);
        Log("{Count} lines got for deletion by age factor", lines.Count());
        foreach (Line line in lines.ToList().Shuffle())
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            await DeleteLineAsync(line);
            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }

        Log("Clear lines by age finsihed");
        CheckTimers(timers);
    }
    public async Task ClearLinesByCountAsync(int maxLines, int maxDeleteLines, CancellationToken stoppingToken, IEnumerable<Timer> timers)
    {
        Log("Clearing lines by count started");
        IEnumerable<Line> lines = GetLinesByCount(_map, maxLines, maxDeleteLines);

        Log("{Count} lines got for deletion by count factor", lines.Count());
        foreach (Line line in lines.ToList().Shuffle())
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            await DeleteLineAsync(line);
            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }

        Log("Clearing lines by count finished");
        CheckTimers(timers);
    }
}