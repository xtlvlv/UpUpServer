using NLog;
using NLog.Targets;

namespace UpUpServer;

public class NLogger : ILog
{
    private NLog.Logger logger;

    public void Init()
    {
        var logPath = ConfigManager.Instance.ServerConfig?.NLogConfigPath ?? @"Conf/nlog.config";
        LogManager.Setup().LoadConfigurationFromFile(logPath);
        
        // 配置文件变更时生效
        LogManager.ConfigurationChanged += (sender, args) =>
        {
            if (args.ActivatedConfiguration != null)
            {
                //Re apply if config reloaded
                UpdateConfig();
            }
        };
        this.logger = LogManager.GetCurrentClassLogger();
    }
    
    public void UpdateConfig()
    {
        // Do NOT set LogManager.Configuration because that will cause recursion and stackoverflow
        // var fileTarget = LogManager.Configuration.FindTargetByName<FileTarget>("myTargetName");
        // fileTarget.FileName = "${basedir}/file.log";
        LogManager.ReconfigExistingLoggers(); // Soft refresh
    }
    
    

    public void Trace(string message, string tag = "")
    {
        this.logger.Trace($"{tag} {message}");
    }

    public void Info(string message, string tag = "")
    {
        this.logger.Info($"{tag} {message}");
    }

    public void Warning(string message, string tag = "")
    {
        this.logger.Warn($"{tag} {message}");
    }

    public void Error(string message, string tag = "")
    {
        this.logger.Error($"{tag} {message}");
    }

    public void Trace(string message, params object[] args)
    {
        this.logger.Trace(message, args);
    }

    public void Info(string message, params object[] args)
    {
        this.logger.Info(message, args);
    }

    public void Warning(string message, params object[] args)
    {
        this.logger.Warn(message, args);
    }

    public void Error(string message, params object[] args)
    {
        this.logger.Error(message, args);
    }

    public void Flush()
    {
    }
}