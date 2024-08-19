namespace UpUpServer;

using System.Collections.Generic;

public class FileLogger : ILog
{
    private string timeFormat = "yyyy-MM-dd HH:mm:ss.fff";
    private string fileFormat = "yyyyMMdd_hhmmss";

    private List<string> logArray;
    private string logPath = null;

    private int currLogCount = 0;
    private string reporterPath = "./";


    private int logBufferMaxNumber = 10;
    private int curLogBufferNumber = 0;
    private int logMaxCapacity = 500;
    private int logFileMaxSize = 1024 * 1024 * 1;

    public void Init()
    {
        logArray = new List<string>();
        reporterPath = ConfigManager.Instance.ServerConfig?.LogPath ?? reporterPath;
        if (string.IsNullOrEmpty(logPath))
        {
            logPath = reporterPath + "//" + DateTime.Now.ToString(fileFormat) + "_log.txt";
        }
    }

    #region 日志接口

    public void Trace(string msg, string tag = "")
    {
        logArray.Add($"[Trace]{DateTime.Now.ToString(timeFormat)} {tag} {msg}");
        CheckWriteFile();
    }

    public void Info(string msg, string tag = "")
    {
        logArray.Add($"[Info]{DateTime.Now.ToString(timeFormat)} {tag} {msg}");
        CheckWriteFile();

    }

    public void Warning(string msg, string tag = "")
    {
        logArray.Add($"[Warning]{DateTime.Now.ToString(timeFormat)} {tag} {msg}");
        CheckWriteFile();

    }

    public void Error(string msg, string tag = "")
    {
        logArray.Add($"[Error]{DateTime.Now.ToString(timeFormat)} {tag} {msg}");
        CheckWriteFile();

    }

    public void Error(Exception e)
    {
        logArray.Add(e.ToString());
        CheckWriteFile();

    }

    public void Trace(string message, params object[] args)
    {
        logArray.Add(String.Format(message, args));
        CheckWriteFile();

    }

    public void Warning(string message, params object[] args)
    {
        logArray.Add(String.Format(message, args));
        CheckWriteFile();

    }

    public void Info(string message, params object[] args)
    {
        logArray.Add(String.Format(message, args));
        CheckWriteFile();

    }

    public void Error(string message, params object[] args)
    {
        logArray.Add(String.Format(message, args));
        CheckWriteFile();

    }

    #endregion

    #region 写入文件操作
    
    public void Flush()
    {
        SyncLog();
    }

    private void CheckWriteFile()
    {
        SyncLog();

        // if (logArray.Count >= logBufferMaxNumber)
        // {
        //     SyncLog();
        // }
    }
    
    private void SyncLog()
    {
        int len = logArray.Count;
        // string logData = string.Join("\r\n", logArray);
        SyncToFile(logPath, "");
        logArray.Clear();
    }

    private void SyncToFile(string pathAndName, string info)
    {
        if (!Directory.Exists(reporterPath)) Directory.CreateDirectory(reporterPath);

        StreamWriter sw;
        FileInfo t = new FileInfo(pathAndName);

        if (!t.Exists)
        {
            sw = t.CreateText();
        }
        else
        {
            if (t.Length > logFileMaxSize)
            {
                logPath = reporterPath + "//" + DateTime.Now.ToString(fileFormat) + "_log.txt";
                t = new FileInfo(logPath);
                sw = t.CreateText();
            }
            else
            {
                sw = t.AppendText();
            }
        }

        // sw.Write(info);
        for (int i = 0; i < logArray.Count; i++)
        {
            sw.WriteLine(logArray[i]);
        }
        sw.Close();
        sw.Dispose();
    }

    [Obsolete]
    private void Write(string writeFileData, LogLevel type)
    {
        if (currLogCount >= logMaxCapacity)
        {
            logPath = reporterPath + "//" + DateTime.Now.ToString(fileFormat) + "_log.txt";
            logMaxCapacity = 0;
        }

        currLogCount++;

        if (!string.IsNullOrEmpty(writeFileData))
        {
            writeFileData = DateTime.Now.ToString(timeFormat) + "|" + type.ToString() + "|" +
                            writeFileData + "\r\n";
            AppendDataToFile(writeFileData);
        }
    }

    [Obsolete]
    private void AppendDataToFile(string writeFileDate)
    {
        if (!string.IsNullOrEmpty(writeFileDate))
        {
            logArray.Add(writeFileDate);
        }

        if (logArray.Count % logBufferMaxNumber == 0)
        {
            SyncLog();
        }
    }

    #endregion
}