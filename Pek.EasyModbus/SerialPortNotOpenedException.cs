using System.Runtime.Serialization;

namespace Pek.EasyModbus;

public class SerialPortNotOpenedException : ModbusException
{
    public SerialPortNotOpenedException()
    {
    }

    public SerialPortNotOpenedException(String message)
      : base(message)
    {
    }

    public SerialPortNotOpenedException(String message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected SerialPortNotOpenedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
}