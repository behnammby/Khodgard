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

    public MapBehavior(int mapId, ILogger logger, UnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;

        _map = _uow.GetMapById(mapId);
        _map.Source.Exchange.Init();
        _map.Target.Exchange.Init();
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
                _uow.LinesRepo.Create(new(line.Price, line.Amount, line.Side, _map));
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

        bool result = await _map.Target.Exchange.CreateOrderAsync(order, _map.Target.PricePrecision, _map.Target.AmountPrecision);
        if (result)
        {
            _uow.LinesRepo.Create(line);
            _uow.OrdersRepo.Create(order);
        }
        else
            return;

        await _uow.CommitAsync();
    }
    private async Task UpdateLineAsync(Line line, double amount)
    {
        if (amount > line.Amount)
        {
            double diff = amount - line.Amount;
            Order order = new(price: line.Price, amount: diff, side: line.Side, market: _map.Target, line);

            bool result = await _map.Target.Exchange.CreateOrderAsync(order, _map.Target.PricePrecision, _map.Target.AmountPrecision);
            if (result)
            {
                line.Amount += diff;
                line.Updated = DateTime.UtcNow;
                _uow.LinesRepo.Update(line);
                _uow.OrdersRepo.Create(order);
            }
            else
                return;
        }
        else if (amount < line.Amount)
        {
            var lineOrders = _uow.GetOrdersOfLine(line);
            double diff = line.Amount - amount;

            if (diff <= 0)
                return;

            double deleteAmount = 0;
            Order order = new(price: line.Price, amount: diff, side: line.Side, market: _map.Target, line: line);

            bool createResult = await _map.Target.Exchange.CreateOrderAsync(order, _map.Target.PricePrecision, _map.Target.AmountPrecision);
            if (createResult)
            {
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

            line.Amount -= deleteAmount;
            if (line.Amount > amount * 1.5)
                if (await _map.Target.Exchange.CancelOrderAsync(order))
                    line.Amount -= amount;

            line.Updated = DateTime.UtcNow;
            _uow.LinesRepo.Update(line);
            _uow.OrdersRepo.Create(order);

            // diff = deleteAmount - diff;
            // if (diff > 0)
            // {
            //     Order order = new(price: line.Price, amount: diff, side: line.Side, _map.Target, line);
            //     bool result = await _map.Target.Exchange.CreateOrderAsync(order, _map.Target.PricePrecision, _map.Target.AmountPrecision);
            //     if (result)
            //     {
            //         line.Amount = amount;
            //         line.Updated = DateTime.UtcNow;
            //         _uow.LinesRepo.Update(line);
            //         _uow.OrdersRepo.Create(order);
            //     }
            //     else
            //         return;
            // }
            // else
            //     return;
        }

        await _uow.CommitAsync();
    }
    private async Task DeleteLineAsync(Line line)
    {
        Log("Deleting line {Id}, pirce= {Price}, amount= {Amount}", line.Id, line.Price, line.Amount);
        var orders = _uow.GetOrdersOfLine(line);
        foreach (Order order in orders)
        {
            bool result = await _map.Target.Exchange.CancelOrderAsync(order);
            if (!result)
                Log("Couldn't delete the order {Order}, price= {Pirce}, amount= {Amount}", order.Uid, order.Price, order.Amount);
            _uow.OrdersRepo.Delete(order);
        }

        _uow.LinesRepo.Delete(line);

        await _uow.CommitAsync();
        Log("Deleted");
    }
    private IEnumerable<Line> GetLinesByAge(Map map, int maxAge) => _uow.LinesRepo.GetLinesByAge(map, maxAge);
    private IEnumerable<Line> GetLinesByCount(Map map, int maxLines) => _uow.LinesRepo.GetLinesByCount(map, maxLines);
    private void Log(string msg, params object?[] args)
    {
        msg = $"Map#{_map.Id}: " + msg;
        _logger.LogInformation(msg, args);
    }
    private void CheckTimers(IEnumerable<Timer> timers)
    {
        if (timers.Any(_ => !_.Enabled))
            throw new TimerStoppedException();
    }

    public async Task SynchronizeLinesAsync(CancellationToken stoppingToken, IEnumerable<Timer> timers)
    {
        if (stoppingToken.IsCancellationRequested)
            return;

        Log("Synchronizing lines started");

        IEnumerable<Line> targetLines = await _map.Target.Exchange.GetDepthAsync(_map.Target, _map);
        Log("{Count} lines got from target, refershing before syncying", targetLines.Count());
        await RefreshLinesAsync(targetLines);

        IEnumerable<Line> sourceLines = await _map.Source.Exchange.GetDepthAsync(_map.Source, _map);
        Log("{Count} lines got from source, sychronizing", sourceLines.Count());
        foreach (Line sourceLine in sourceLines.ToList().Shuffle())
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            Log("Syncing line Price= {Line}, Amount= {Amount}, Side= {Side}", sourceLine.Price, sourceLine.Amount, sourceLine.Side);
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
            Log("Syncying finished");
        }

        Log("Synchronizying lines finished.");
        CheckTimers(timers);
    }
    public async Task ClearLinesByAgeAsync(int maxAge, CancellationToken stoppingToken, IEnumerable<Timer> timers)
    {
        Log("Clear lines by age started");

        IEnumerable<Line> targetLines = await _map.Target.Exchange.GetDepthAsync(_map.Target, _map);
        Log("{Count} lines got from target, refershing before deleting", targetLines.Count());
        await RefreshLinesAsync(targetLines);

        IEnumerable<Line> lines = GetLinesByAge(_map, maxAge);
        Log("{Count} lines got for deletion by age factor", lines.Count());
        foreach (Line line in lines.ToList().Shuffle())
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            await DeleteLineAsync(line);
        }

        Log("Clear lines by age finsihed");
        CheckTimers(timers);
    }
    public async Task ClearLinesByCountAsync(int maxLines, CancellationToken stoppingToken, IEnumerable<Timer> timers)
    {
        Log("Clearing lines by count started");
        IEnumerable<Line> lines = GetLinesByCount(_map, maxLines);

        Log("{Count} lines got for deletion by count factor", lines.Count());
        foreach (Line line in lines.ToList().Shuffle())
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            await DeleteLineAsync(line);
        }

        Log("Clearing lines by count finished");
        CheckTimers(timers);
    }
}