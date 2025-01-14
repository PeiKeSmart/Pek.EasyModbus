using System.Runtime.Serialization;

namespace Pek.EasyModbus;

public class QuantityInvalidException : ModbusException
{
    public QuantityInvalidException()
    {
    }

    public QuantityInvalidException(String message)
      : base(message)
    {
    }

    public QuantityInvalidException(String message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected QuantityInvalidException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
}