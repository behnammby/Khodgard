using System.Runtime.Serialization;

namespace Khodgard.Exceptions;

public class CreateOrderException : ApplicationException
{
    public CreateOrderException()
    {
    }

    public CreateOrderException(string? message) : base(message)
    {
    }

    public CreateOrderException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected CreateOrderException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}