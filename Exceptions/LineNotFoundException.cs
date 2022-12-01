using System.Runtime.Serialization;

namespace Khodgard.Exceptions;

public class LineNotFoundException : ApplicationException
{
    public LineNotFoundException()
    {
    }

    public LineNotFoundException(string? message) : base(message)
    {
    }

    public LineNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected LineNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}