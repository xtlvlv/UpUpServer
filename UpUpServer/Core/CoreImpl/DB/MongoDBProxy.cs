using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace UpUpServer;

public class MongoDBProxy: SingleClass<MongoDBProxy>
{
    public MongoClient Client { get; private set; }
    public IMongoDatabase CurDB { get; private set; }
    
    public void Open(string url, string dbName)
    {
        try
        {
            var settings = MongoClientSettings.FromConnectionString(url);
            Client = new MongoClient(settings);
            CurDB = Client.GetDatabase(dbName);
            Log.Info($"Init MongoDB Success Url:{url} DbName:{dbName}");
        }
        catch (Exception)
        {
            Log.Error($"Init MongoDB Fail Url:{url} DbName:{dbName}");
            throw;
        }
    }
    public void Close()
    {
        Client.Cluster.Dispose();
    }

}