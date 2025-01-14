using System.Runtime.Serialization;

namespace Pek.EasyModbus;

public class FunctionCodeNotSupportedException : ModbusException
{
    public FunctionCodeNotSupportedException()
    {
    }

    public FunctionCodeNotSupportedException(String message)
      : base(message)
    {
    }

    public FunctionCodeNotSupportedException(String message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected FunctionCodeNotSupportedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
}