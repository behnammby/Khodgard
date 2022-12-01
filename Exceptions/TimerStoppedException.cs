using System.Runtime.Serialization;

namespace Khodgard.Exceptions;

public class TimerStoppedException : ApplicationException
{
    public TimerStoppedException()
    {
    }

    public TimerStoppedException(string? message) : base(message)
    {
    }

    public TimerStoppedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected TimerStoppedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}