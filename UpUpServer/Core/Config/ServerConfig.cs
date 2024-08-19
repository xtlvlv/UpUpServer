namespace UpUpServer;

public class ServerConfig
{
    public string Ip;
    public int Port;
    public int WorkPoolSize;
    public string LogPath;
    
    public string NLogConfigPath;
    
    public string MongoDbUrl;   // 数据库地址
    public string MongoDbName; // 数据库名
    
    public string RedisDbUrl; // redis地址

}