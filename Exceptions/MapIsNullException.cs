using System.Runtime.Serialization;

namespace Khodgard.Exceptions;

public class MapIsNullException : ApplicationException
{
    public MapIsNullException()
    {
    }

    public MapIsNullException(string? message) : base(message)
    {
    }

    public MapIsNullException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected MapIsNullException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}