using System.Buffers;
using System.Text;

public class TcpClient
{
    public MsgManager MsgManager { get; set; }
    public Connection Connection { get; set; }
    public string Ip { get; set; }
    public int Port { get; set; }
    

    public TcpClient(string ip, int port)
    {
        Ip = ip;
        Port = port;
        Connection = new Connection(Ip, Port);
        MsgManager = new MsgManager();
    }
    
    public void Start()
    {
        Connection.Connect();
        Connection.OnReceived += HandleMsg;
        MsgManager.StartWorkPool();
    }

    private void HandleMsg(ReadOnlySequence<byte> obj)
    {
        var netPackage = UpPackSerialize.Instance.Deserialize(obj);
        if (netPackage is null)
        {
            return;
        }
        MsgManager.AddResponse(netPackage);
    }
    
    public void SetOnConnected(Action action)
    {
        Connection.OnConnected += action;
    }
    
    public void SetOnReceived(Action<ReadOnlySequence<byte>> action)
    {
        Connection.OnReceived += action;
    }
    
    public void SetOnClose(Action<string> action)
    {
        Connection.OnClose += action;
    }

    public void Send<T>(uint msgId, T data)
    {
        var input = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        
        var netPack = new UpNetPackage();
        netPack.Data = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(input));
        netPack.MsgId = msgId;
        netPack.AllLength = (uint)netPack.Data.Length + 4 + 4;

        Memory<byte> bytesData = new byte[netPack.AllLength];
        UpPackSerialize.Instance.Serialize(netPack, ref bytesData);

        Connection.Send(bytesData.Span);
    }
    
    public void Send(Span<byte> data)
    {
        Connection.Send(data);
    }
    
    public async Task SendAsync(Memory<byte> data)
    {
        await Connection.SendAsync(data);
    }
    
    public void AddListener(uint msgId, Action<UpNetPackage> action)
    {
        MsgManager.RegisterRouter(msgId, action);
    }
    
    public void Close()
    {
        Connection.Close();
    }
}