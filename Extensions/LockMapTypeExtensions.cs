using Khodgard.Enumerations;

namespace Khodgard.Extensions;

public static class LockMapTypeExtensions
{
    public static string ToColumnName(this LockMapType lockType) => "LockedFor" + lockType.ToString();
}