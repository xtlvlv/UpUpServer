using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using ServiceStack.Redis;
using static System.Console;

namespace UpUpServer;

public class ServerTest
{
    public class Role
    {
        public int                      Id;
        public string                   Name;
        public Dictionary<string, Role> Childs = new Dictionary<string, Role>();
    }

    public class Player
    {
        public ulong                    Uid;    
        public string                   UserId; 
        public ulong                    DataVersion;
        public List<int>                RoleProfessionIds = new List<int>(); 
        public Dictionary<string, Role> Roles             = new Dictionary<string, Role>();         
        public Role                     DataRole = new Role(); 

    }
  
    public static void RunServerTest()
    {
        WriteLine("ServerTest");
        Server server = new Server();
        server.Start();

        ReadKey();
    }

    public static void TestEndian()
    {
        var a = 0x12345678;
        var b = BitConverter.GetBytes(a);
        var c = BitConverter.ToInt32(b);
        WriteLine($"a:{a}, b:{b}, c:{c}");

        if (BitConverter.IsLittleEndian)
        {
            WriteLine("当前系统是小端序");
        }
        else
        {
            WriteLine("当前系统是大端序");
        }
    }
    
    public static void TestMongo()
    {
        var mongo = new MongoDBProxy();
        mongo.Open("mongodb://localhost:27017", "GameTest");
        var userdb = mongo.CurDB.ListCollectionNames();
        userdb.MoveNext();
        var l =userdb.Current.ToList();
        foreach (var a in l)
        {
            Log.Info(a);
        }

        try
        {
            var user = mongo.CurDB.GetCollection<BsonDocument>("User");
            // 取出所有数据
            var users = user.AsQueryable().ToList();
            foreach (var u in users)
            {
                Log.Info(u.ToString());
            }
        }
        catch (Exception e)
        {
            WriteLine(e);
            throw;
        }
        
        // mongo.CurDB.CreateCollection("GameUser");
        var gameUser = mongo.CurDB.GetCollection<GameUser>("GameUser");
        gameUser.AsQueryable().ToList().ForEach(u => Log.Info($"name:{u.Name} level:{u.Level} {u.ToString()}"));
    }
    
    class GameUser
    {
        public string Name;
        public int Level;
    }

    public static void TestRedis()
    {
        RedisClient redisClient = new RedisClient("127.0.0.1", 6379);//redis服务IP和端口
        var res = redisClient.HGetAll("testkey");
        Console.WriteLine(res);

        
        var redisManager = new RedisManagerPool("127.0.0.1:6379");
        using (var client = redisManager.GetClient())
        {
            var res2 = client.GetAllEntriesFromHash("testkey");
            foreach (var r in res2)
            {
                Log.Info($"key:{r.Key} value:{r.Value}");
            }
        }
        
    }
   
}