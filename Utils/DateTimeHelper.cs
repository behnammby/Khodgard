namespace Khodgard.Utils;

public static class DateTimeHelper
{
    public static double ToUnixTimestamp(DateTime dt)
    {
        DateTime basePoint = new(year: 1970, month: 1, day: 1);
        TimeSpan ts = dt.Subtract(basePoint);

        return ts.TotalSeconds;
    }
    public static double ToUnixTimestamp() => ToUnixTimestamp(DateTime.UtcNow);
}