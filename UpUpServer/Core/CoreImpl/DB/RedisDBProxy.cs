using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ServiceStack;
using ServiceStack.Redis;

namespace UpUpServer;

public class RedisDBProxy: SingleClass<RedisDBProxy>
{
    public RedisManagerPool RedisPool { get; private set; } = null!;
    public IRedisClient? RedisClient;

    public void Open(string url)
    {
        try
        {
            RedisPool = new RedisManagerPool(url);
            // RedisClient = RedisPool.GetClient();
            Log.Info($"Init Redis Success Url:{url}");
        }
        catch (Exception)
        {
            Log.Error($"Init Redis Fail Url:{url}");
            throw;
        }
    }
    public void Close()
    {
        RedisPool.Dispose();
    }

}