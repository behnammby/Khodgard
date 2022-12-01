using System.Runtime.Serialization;

namespace Khodgard.Exceptions;

public class LockMapTypeInvalidException : ApplicationException
{
    public LockMapTypeInvalidException()
    {
    }

    public LockMapTypeInvalidException(string? message) : base(message)
    {
    }

    public LockMapTypeInvalidException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected LockMapTypeInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
