namespace Pek.EasyModbus;

public sealed class StoreLogData
{
    private String? filename;
    private static volatile StoreLogData? instance;
    private static Object syncObject = new();

    private StoreLogData()
    {
    }

    public static StoreLogData Instance
    {
        get
        {
            if (instance == null)
            {
                lock (syncObject)
                {
                    instance ??= new StoreLogData();
                }
            }
            return instance;
        }
    }

    public void Store(String message)
    {
        if (filename == null)
            return;
        using var streamWriter = new StreamWriter(Filename!, true);
        streamWriter.WriteLine(message);
    }

    public void Store(String message, DateTime timestamp)
    {
        try
        {
            using var streamWriter = new StreamWriter(Filename!, true);
            streamWriter.WriteLine(timestamp.ToString("dd.MM.yyyy H:mm:ss.ff ") + message);
        }
        catch
        {
        }
    }

    public String? Filename
    {
        get => filename;
        set => filename = value;
    }
}