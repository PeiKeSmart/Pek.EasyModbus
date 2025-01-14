using System.Runtime.Serialization;

namespace Pek.EasyModbus;

public class CRCCheckFailedException : ModbusException
{
    public CRCCheckFailedException()
    {
    }

    public CRCCheckFailedException(String message)
      : base(message)
    {
    }

    public CRCCheckFailedException(String message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected CRCCheckFailedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
}