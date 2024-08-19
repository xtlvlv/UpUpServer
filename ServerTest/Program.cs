using UpUpServer;
using System.Collections;

class Program
{
    public static Server Server;
    public static ulong Count = 0;

    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, UpUpServer!");
        AppRun();
        // ServerTest.TestEndian();
    }

    private static void AppRun()
    {
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            Log.Flush();
            // 正常退出
            OnProcessExit(s, e);
        };

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            Log.Flush();
            OnFetalException(e.ExceptionObject);
            // OnProcessExit(s, e);
        };

        ConfigManager.Instance.Init();
        
        Logger.Instance.ILog = new NLogger();

        Server = new Server();
        Server.Start();

        while (true)
        {
            // Log.Info($"RunCount={Count}");
            Thread.Sleep(10000);
        }
    }

    private static void OnProcessExit(object sender, EventArgs e)
    {
        Log.Info($"app exit");
        Server.Stop();
    }


    private static void OnFetalException(object e)
    {
        Log.Error("UnhandledException exit");
        if (e is IEnumerable arr)
        {
            foreach (var ex in arr)
                Log.Error($"Unhandled Exception:{ex}");
        }
        else
        {
            Log.Error($"Unhandled Exception:{e}");
        }
    }
}