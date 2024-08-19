namespace UpUpServer;

public class ConsoleLogger : ILog
{
    private string timeFormat = "yyyy-MM-dd HH:mm:ss.fff";

    public void Init()
    {
    }

    public void Trace(string msg, string tag = "")
    {
        Console.WriteLine($"{tag} {msg}");
    }

    public void Debug(string msg, string tag = "")
    {
        Console.WriteLine($"{tag} {msg}");
    }

    public void Info(string msg, string tag = "")
    {
        Console.WriteLine($"[Info]{DateTime.Now.ToString(timeFormat)} {tag} {msg}");
    }

    public void Warning(string msg, string tag = "")
    {
        Console.WriteLine($"[Warning]{DateTime.Now.ToString(timeFormat)} {tag} {msg}");
    }

    public void Error(string msg, string tag = "")
    {
        Console.WriteLine($"[Error]{DateTime.Now.ToString(timeFormat)} {tag} {msg}");
    }

    public void Error(Exception e)
    {
        Console.WriteLine(e);
    }

    public void Trace(string message, params object[] args)
    {
        Console.WriteLine(String.Format(message, args));
    }

    public void Warning(string message, params object[] args)
    {
        Console.WriteLine(String.Format(message, args));
    }

    public void Info(string message, params object[] args)
    {
        Console.WriteLine(message, args);
    }

    public void Debug(string message, params object[] args)
    {
        Console.WriteLine(message, args);
    }

    public void Error(string message, params object[] args)
    {
        Console.WriteLine(message, args);
    }

    public void Flush()
    {
    }
}