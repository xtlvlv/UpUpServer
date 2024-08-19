using MongoDB.Driver;
using ServerTest.Const;
using ServiceStack.Redis;
using UpUpServer;


[AutoRouter(ProtoDefine.LoginReq)]
public class LoginRouter: JsonRpcRouter<LoginRequest, LoginResponse>
{
    private IMongoCollection<User>?   _collection;
    private IRedisClient?             _redisClient;

    public override void PreHandle(IRequest request)
    {
        base.PreHandle(request);
        if (this._collection is null)
        {
            var mongoDb = MongoDBProxy.Instance.CurDB;
            _collection = mongoDb.GetCollection<User>("User");
        }

        // if (_redisClient is null)
        // {
        //     _redisClient    = RedisDBProxy.Instance.RedisPool.GetClient();
        //     _redisClient.Db = 0;
        // }
    }
    public override async Task Handle(IRequest request)
    {
        await base.Handle(request);
        var req = (LoginRequest)request.Req!;
        var res = (LoginResponse)request.Res;
        
        request.Connection.SetUserId(req.UserId);

        res.Data = "";

        var data = await _collection.FindAsync(x => x.Id == request.Connection.UserId);
        var user = data.FirstOrDefault();
        if (user == null)
        {
            user = new User()
            {
                Id          = request.Connection.UserId,
                Uid         = (ulong)(Random.Shared.NextInt64()),
                Password    = req.Password,
                Data        = "",
                DataVersion = 0,
            };
            if (_collection != null) await _collection.InsertOneAsync(user);
            res.Status = ResponseDefine.LoginNewUser;
            res.Data = "";
            res.Uid = user.Uid;
            res.DataVersion = 0;
            Log.Warning($"new user userId={request.Connection.UserId}");
        }
        else
        {
            res.Status = ResponseDefine.Success;
            res.Data = user.Data;
            res.Uid = user.Uid;
            res.DataVersion = user.DataVersion;
            Log.Info($"user login userId={request.Connection.UserId}");
        }
    }
}