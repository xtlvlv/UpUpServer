using System.Reflection;

namespace UpUpServer;

using System.Net;
using System.Net.Sockets;

public class Server : IServer
{
    private Socket _listener = null!;
    private string _ip = "127.0.0.1";
    private int _port = 9999;
    private IPEndPoint _ipEndPoint = null!;
    private uint curId = 0;
    public bool IsRunning { get; set; }

    public IMsgManager MsgManager { get; set; }
    public IConnManager ConnManager { get; set; }
    public float RunTime = 0;

    public SystemManager SysManager { get; set; }

    // 更新频率 ms
    public int UpdateInterval = 200;
    
    public Server()
    {
        ConnManager = new ConnManager();
        MsgManager = new MsgManager();
        SysManager = new SystemManager();
    }

    public async void Start()
    {
        Log.Info($"server starting...");
        IsRunning = true;
        InitReflect();
        
        MsgManager.StartWorkPool(this);
        
        if (ConfigManager.Instance.ServerConfig?.MongoDbUrl!=null && ConfigManager.Instance.ServerConfig.MongoDbUrl!="")
        {
            MongoDBProxy.Instance.Open(ConfigManager.Instance.ServerConfig.MongoDbUrl, ConfigManager.Instance.ServerConfig.MongoDbName);
        }
        else
        {
            Log.Error($"mongo db url is null");
        }
        if (ConfigManager.Instance.ServerConfig?.RedisDbUrl!=null && ConfigManager.Instance.ServerConfig.RedisDbUrl!="")
        {
            RedisDBProxy.Instance.Open(ConfigManager.Instance.ServerConfig.RedisDbUrl);
        }
        else
        {
            Log.Error($"redis db url is null");
        }
        
        _ip = ConfigManager.Instance.ServerConfig?.Ip ?? _ip;
        _port = ConfigManager.Instance.ServerConfig?.Port ?? _port;

        _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _ipEndPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);
        _listener.Bind(_ipEndPoint);
        _listener.Listen(3000);
        Log.Info($"server start at {_ip}:{_port}");
        
        // 监听连接
        new Thread(AcceptAsync).Start();
        // 不可靠的心跳检测
        new Thread(CheckConnection).Start();

        TimerManager.Instance.Init();
        
        // 2s 调用一次
        UpUpServer.Timer.LoopAction(UpdateInterval, (_ =>
        {
            this.Update();
        }));
    }

    public void InitReflect()
    {
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
        var routerTypes = types.Where(t =>
            t.GetCustomAttributes(typeof(AutoRouterAttribute), false).Length > 0);
        
        foreach (var type in routerTypes)
        {
            var attribute = type.GetCustomAttribute<AutoRouterAttribute>();
            this.AddRouter(attribute.ReqId, Activator.CreateInstance(type) as IRouter);
            Log.Info($"Add Router: {type.FullName} ReqId: {attribute.ReqId}");
        }
        
        var systemTypes = types.Where(t =>
            t.GetCustomAttributes(typeof(AutoSystemAttribute), false).Length > 0);
        foreach (var typeSys in systemTypes)
        {
            var attribute = typeSys.GetCustomAttribute<AutoSystemAttribute>();
            this.AddSystem(typeSys, Activator.CreateInstance(typeSys) as ISystem);
            Log.Info($"Add System: {typeSys.FullName}");
        }
    }

    public void AcceptAsync()
    {
        while (IsRunning)
        {
            Socket clientSocket = _listener.Accept();
            Log.Info($"client connected: {clientSocket.RemoteEndPoint}");

            Interlocked.Increment(ref curId);

            Connection connection = new Connection(curId, clientSocket, this.MsgManager);
            ConnManager.AddConnection(connection);
            connection.Start();
        }
    }

    public void CheckConnection()
    {
        while (IsRunning)
        {
            //每10s 检查一次
            Thread.Sleep(1000 * 10);
            ConnManager.CheckOnline();
        }
    }
    
    public void Update()
    {
        SysManager.Update();
    }

    public void Stop()
    {
        Log.Info($"server stoping...");
        _listener.Close();
        _listener.Dispose();

        Log.Info($"server stoped");
    }

    public void AddRouter(uint msgId, IRouter router)
    {
        MsgManager.RegisterRouter(msgId, router);
    }

    public void AddSystem(Type type, ISystem system)
    {
        SysManager.RegistSystem(type, system);
    }

    
}