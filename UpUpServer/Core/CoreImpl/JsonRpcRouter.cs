using System.Buffers;

namespace UpUpServer;

using Newtonsoft.Json;
using System.Text;

public abstract class JsonRpcRouter<T1, T2> : IRouter
    where T1 : class, IProtocol
    where T2 : class, IProtocol, new()
{
    public int ErrorCode { get; set; }
    public IMsgManager MsgManager { get; set; } = null!; // 添加路由时赋值  
    public virtual void PreHandle(IRequest request)
    {
        ErrorCode = 0;
        try
        {
            var rawData = Encoding.UTF8.GetString(request.NetPackage.Data);
            request.Req = JsonConvert.DeserializeObject<T1>(rawData) as T1;
            if (request.Req is null)
            {
                Log.Error($"JsonRpcRouter.PreHandle: {rawData} is not {typeof(T1).Name}");
                ErrorCode = 1;
                return;
            }
        }
        catch (Exception e)
        {
            Log.Error($"net data paser error exception={e}");
            ErrorCode = 1;
            return;
        }
        
        request.Res = new T2();
    }

    public virtual Task Handle(IRequest request)
    {
        return Task.CompletedTask;
    }

    public virtual void PostHandle(IRequest request)
    {
        if (ErrorCode != 0 || request.Res is NoResponse)
        {
            return;
        }
        var res = request.Res as T2;
        if (res is null)
        {
            Log.Error($"JsonRpcRouter.PostHandle: {request.Res} is not {typeof(T2).Name}");
            return;
        }
        var resData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res));
        request.ResNetPackage = new UpNetPackage
        {
            MsgId = request.NetPackage.MsgId+1,
            Data = new ReadOnlySequence<byte>(resData),
        };
        request.ResNetPackage.AllLength = (uint)resData.Length+4+4;
        MsgManager.AddResponse(request);
    }
}