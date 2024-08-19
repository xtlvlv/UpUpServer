namespace UpUpServer;

public static class Log
{
    public static void Trace(string msg)
    {
        Logger.Instance.Trace(msg);
    }

    public static void Info(string msg)
    {
        Logger.Instance.Info(msg);
    }

    public static void Warning(string msg)
    {
        Logger.Instance.Warning(msg);
    }

    public static void Error(string msg)
    {
        Logger.Instance.Error(msg);
    }

    public static void Error(Exception e)
    {
        Logger.Instance.Error(e.Message);
    }

    public static void Trace(string message, params object[] args)
    {
        Logger.Instance.Trace(message, args);
    }

    public static void Warning(string message, params object[] args)
    {
        Logger.Instance.Warning(string.Format(message, args));
    }

    public static void Info(string message, params object[] args)
    {
        Logger.Instance.Info(string.Format(message, args));
    }


    public static void Error(string message, params object[] args)
    {
        Logger.Instance.Error(message, args);
    }
    
    public static void Flush()
    {
        Logger.Instance.Flush();
    }
}