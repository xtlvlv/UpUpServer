using System.Buffers;
using System.Threading.Channels;

public class MsgManager
{
    public Dictionary<uint, Action<UpNetPackage>> Routers { get; } = new Dictionary<uint, Action<UpNetPackage>>();
    public Channel<UpNetPackage>[] RequestChannel { get; set; }
    public Channel<UpNetPackage>[] ResponseChannel { get; set; }
    public TcpClient Client { get; set; }

    public int WorkPoolSize = 2;

    public void StartWorkPool()
    {
        RequestChannel = new Channel<UpNetPackage>[WorkPoolSize];
        for (int i = 0; i < WorkPoolSize; i++)
        {
            RequestChannel[i] = Channel.CreateUnbounded<UpNetPackage>();
            var i1 = i;
            Task.Run(() => ReadWorker(i1));
        }
        
        ResponseChannel = new Channel<UpNetPackage>[WorkPoolSize];
        for (int i = 0; i < WorkPoolSize; i++)
        {
            ResponseChannel[i] = Channel.CreateUnbounded<UpNetPackage>();
            var i1 = i;
            Task.Run(() => WriteWorker(i1));
        }
    }

    private async void ReadWorker(int index)
    {
        Console.WriteLine($"Start ReadWorker {index}");
        var reader = ResponseChannel[index].Reader;
        while (true)
        {
            var request = await reader.ReadAsync();
            HandleMsg(request.MsgId, request);
        }
    }
    
    private async void WriteWorker(int index)
    {
        Console.WriteLine($"Start WriteWorker {index}");
        var reader = RequestChannel[index].Reader;
        while (true)
        {
            var netPack = await reader.ReadAsync();
            var data = ArrayPool<byte>.Shared.Rent((int)netPack.AllLength);
            Memory<byte> memory = new Memory<byte>(data);
            UpPackSerialize.Instance.Serialize(netPack, ref memory);
            await Client.SendAsync(memory.Slice(0, (int)netPack.AllLength));
            ArrayPool<byte>.Shared.Return(data);
        }
    }

    public void AddRequest(UpNetPackage request)
    {
        var msgId = request.MsgId;
        var index = msgId % WorkPoolSize;
        RequestChannel[index].Writer.TryWrite(request);
    }
    
    public void AddResponse(UpNetPackage request)
    {
        var msgId = request.MsgId;
        var index = msgId % WorkPoolSize;
        ResponseChannel[index].Writer.TryWrite(request);
    }

    public void RegisterRouter(uint msgId, Action<UpNetPackage> router)
    {
        if (Routers.TryGetValue(msgId, out var r))
        {
            Console.WriteLine($"router msgId:{msgId} has been registered");
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

    public void HandleMsg(uint msgId, UpNetPackage request)
    {
        if (Routers.TryGetValue(msgId, out var router))
        {
            // router.Invoke(request);
            MainThreadSynchronizationContext.Instance.Post((_) =>
            {
                router.Invoke(request);
            });
        }
        else
        {
            Console.WriteLine($"router msgId:{msgId} not found");
        }
    }
}