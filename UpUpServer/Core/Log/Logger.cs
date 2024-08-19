namespace UpUpServer;

using System;
using System.Diagnostics;

public class Logger
{
    public static Logger Instance = new Logger();

    private ILog iLog = new ConsoleLogger();

    public ILog ILog
    {
        set
        {
            this.iLog = value;
            this.iLog.Init();
        }
    }

    private int logLevel = LogLevel.Info;

    public Logger()
    {
        this.iLog.Init();
    }

    private bool CheckLogLevel(int level)
    {
        return logLevel <= level;
    }

    public void Trace(string msg, string tag = "")
    {
        if (!CheckLogLevel(LogLevel.Trace))
        {
            return;
        }

        StackTrace st = new StackTrace(2, true);
        this.iLog.Trace($"{msg}\n{st}", tag);
    }

    public void Info(string msg, string tag = "")
    {
        if (!CheckLogLevel(LogLevel.Info))
        {
            return;
        }

        this.iLog.Info(msg, tag);
    }

    public void Warning(string msg, string tag = "")
    {
        if (!CheckLogLevel(LogLevel.Warning))
        {
            return;
        }

        this.iLog.Warning(msg, tag);
    }

    public void Error(string msg, string tag = "")
    {
        // StackTrace st = new StackTrace(2, true);
        // this.iLog.Error($"{tag} {msg}\n{st}");
        this.iLog.Error(msg, tag);
    }

    public void Error(Exception e)
    {
        if (e.Data.Contains("StackTrace"))
        {
            this.iLog.Error($"{e.Data["StackTrace"]}\n{e}");
            return;
        }

        string str = e.ToString();
        this.iLog.Error(str);
    }

    public void Trace(string message, params object[] args)
    {
        if (!CheckLogLevel(LogLevel.Trace))
        {
            return;
        }

        StackTrace st = new StackTrace(2, true);
        this.iLog.Trace($"{string.Format(message, args)}\n{st}");
    }

    public void Warning(string message, params object[] args)
    {
        if (!CheckLogLevel(LogLevel.Warning))
        {
            return;
        }

        this.iLog.Warning(string.Format(message, args));
    }

    public void Info(string message, params object[] args)
    {
        if (!CheckLogLevel(LogLevel.Info))
        {
            return;
        }

        this.iLog.Info(string.Format(message, args));
    }

    public void Error(string message, params object[] args)
    {
        StackTrace st = new StackTrace(2, true);
        string s = string.Format(message, args) + '\n' + st;
        this.iLog.Error(s);
    }
    
    public void Flush()
    {
        this.iLog.Flush();
    }
}