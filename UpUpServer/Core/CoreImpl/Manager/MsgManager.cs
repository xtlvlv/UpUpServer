using System.Buffers;
using System.Text;
using System.Threading.Channels;
using Newtonsoft.Json;

namespace UpUpServer;

public class MsgManager : IMsgManager
{
    public Dictionary<uint, IRouter> Routers { get; } = new Dictionary<uint, IRouter>();

    public Channel<IRequest>[] RequestChannel { get; set; }

    public Channel<IRequest>[] ResponseChannel { get; set; }

    public int WorkPoolSize = 4;
    
    public IServer Server { get; set; }

    public void StartWorkPool(IServer server)
    {
        Server = server;
        if (ConfigManager.Instance.ServerConfig == null || ConfigManager.Instance.ServerConfig.WorkPoolSize <= 0)
        {
            Log.Warning($"WorkPoolSize is not set, use default value:{WorkPoolSize}");
        }
        else
        {
            WorkPoolSize = ConfigManager.Instance.ServerConfig.WorkPoolSize;
        }

        RequestChannel = new Channel<IRequest>[WorkPoolSize];
        for (int i = 0; i < WorkPoolSize; i++)
        {
            RequestChannel[i] = Channel.CreateUnbounded<IRequest>();
            var i1 = i;
            Task.Run(() => ReadWorker(i1));
        }
        
        ResponseChannel = new Channel<IRequest>[WorkPoolSize];
        for (int i = 0; i < WorkPoolSize; i++)
        {
            ResponseChannel[i] = Channel.CreateUnbounded<IRequest>();
            var i1 = i;
            Task.Run(() => WriteWorker(i1));
        }
    }

    private async void ReadWorker(int index)
    {
        Log.Info($"Start ReadWorker {index}");
        var reader = RequestChannel[index].Reader;
        while (Server.IsRunning)
        {
            var request = await reader.ReadAsync();
            Log.Info($"receive msgId:{request.NetPackage.MsgId}");
            await HandleMsg(request.NetPackage.MsgId, request);
        }
    }
    
    private async void WriteWorker(int index)
    {
        Log.Info($"Start WriteWorker {index}");
        var reader = ResponseChannel[index].Reader;
        while (Server.IsRunning)
        {
            var request = await reader.ReadAsync();
            var netPack = request.ResNetPackage;
            // Memory<byte> data = new byte[netPack.AllLength];
            var data = ArrayPool<byte>.Shared.Rent((int)netPack.AllLength);
            var memory = new Memory<byte>(data);
            UpPackSerialize.Instance.Serialize(netPack, ref memory);
            await request.Connection.SendAsync(memory.Slice(0, (int)netPack.AllLength));
            // request.Connection.Send(memory.Slice(0, (int)netPack.AllLength));
            ArrayPool<byte>.Shared.Return(data);
        }
    }

    public void AddRequest(IRequest request)
    {
        var msgId = request.NetPackage.MsgId;
        var index = msgId % WorkPoolSize;
        RequestChannel[index].Writer.TryWrite(request);
    }
    
    public void AddResponse(IRequest request)
    {
        var msgId = request.NetPackage.MsgId;
        var index = msgId % WorkPoolSize;
        ResponseChannel[index].Writer.TryWrite(request);
    }
    
    public void AddPackResponse(IRequest request, uint msgId)
    {
        var resData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request.Res));
        request.ResNetPackage = new UpNetPackage
        {
            MsgId = msgId,
            Data = new ReadOnlySequence<byte>(resData),
        };
        request.ResNetPackage.AllLength = (uint)resData.Length+4+4;
        
        var index = msgId % WorkPoolSize;
        ResponseChannel[index].Writer.TryWrite(request);
    }

    public void RegisterRouter(uint msgId, IRouter router)
    {
        router.MsgManager = this;
        if (Routers.TryGetValue(msgId, out var r))
        {
            Log.Warning($"router msgId:{msgId} has been registered");
        }
        else
        {
            Routers.Add(msgId, router);
        }
    }

    public void UnRegisterRouter(uint msgId)
    {
        Routers.Remove(msgId);
    }

    public async Task HandleMsg(uint msgId, IRequest request)
    {
        if (Routers.TryGetValue(msgId, out var router))
        {
            router.PreHandle(request);
            if (router.ErrorCode != 0)
            {
                Log.Error($"router msgId:{msgId} stop after prehandle, ErrorCode={router.ErrorCode}");
                return;
            }
            await router.Handle(request);
            router.PostHandle(request);
        }
        else
        {
            Log.Error($"router msgId:{msgId} not found");
        }
    }
}