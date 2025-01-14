using System.Runtime.Serialization;

namespace Pek.EasyModbus;

public class ConnectionException : ModbusException
{
    public ConnectionException()
    {
    }

    public ConnectionException(String message)
      : base(message)
    {
    }

    public ConnectionException(String message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected ConnectionException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
}