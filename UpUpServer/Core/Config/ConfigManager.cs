namespace UpUpServer;

using Newtonsoft.Json;

public class ConfigManager : SingleClass<ConfigManager>
{
    public ServerConfig? ServerConfig;

    public ConfigManager()
    {
    }

    public void Init()
    {
        ServerConfig = JsonConvert.DeserializeObject<ServerConfig>(File.ReadAllText(BaseDefine.ServerConfigPath));
        if (ServerConfig is null)
        {
            Log.Error($"ServerConfig DeserializeObject failed");
            Environment.Exit(0);
        }
    }
}