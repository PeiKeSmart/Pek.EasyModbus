using System.Runtime.Serialization;

namespace Pek.EasyModbus;

public class ModbusException : Exception
{
    public ModbusException()
    {
    }

    public ModbusException(String message)
      : base(message)
    {
    }

    public ModbusException(String message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected ModbusException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
}