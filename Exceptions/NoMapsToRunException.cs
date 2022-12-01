using System.Runtime.Serialization;

namespace Khodgard.Exceptions;

public class NoMapsToRunException : ApplicationException
{
    public NoMapsToRunException()
    {
    }

    public NoMapsToRunException(string? message) : base(message)
    {
    }

    public NoMapsToRunException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected NoMapsToRunException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}