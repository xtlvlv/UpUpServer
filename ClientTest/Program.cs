using System.Buffers;
using System.Text;
using Newtonsoft.Json;
using ServiceStack;
using UpUpServer;

string ip = "127.0.0.1";
// string ip = "101.42.178.138";
int port = 9999;
var tcpClient = new TcpClient(ip, port);
string userId = "1";


// tcpClient.SetOnReceived(data =>
// {
//     Console.WriteLine("客户端收到了服务端的全部消息: {0}", Encoding.UTF8.GetString(data));
// });

// 其他线程的消息回调，需要设置主线程的同步上下文
SynchronizationContext.SetSynchronizationContext(MainThreadSynchronizationContext.Instance);


Task.Run(() =>
{
    while (true)
    {
        MainThreadSynchronizationContext.Instance.Update();
        Thread.Sleep(10);
    }
});

Connect();

PrintTips();

while (true)
{
    var input = Console.ReadLine();
    if (input == "0")
    {
        break;
    }
    DisptachInput(input);
    Thread.Sleep(2);
}

void Connect()
{
    tcpClient.SetOnConnected(() =>
    {
        Console.WriteLine("客户端连上了服务端");
    });
    tcpClient.SetOnClose(reason =>
    {
        Console.WriteLine("客户端断开了服务端: {0}", reason); 
    });
    AddListener();
    tcpClient.Start();
    
}

void AddListener()
{
    tcpClient.AddListener(2, data =>
    { 
        // 打印当前线程id
        Console.WriteLine($"线程：{Thread.CurrentThread.ManagedThreadId} 客户端处理服务端的消息: {Encoding.UTF8.GetString(data.Data)}");
    });
    tcpClient.AddListener(ProtoDefine.LoginRes, data =>
    { 
        // 打印当前线程id
        Console.WriteLine($"线程：{Thread.CurrentThread.ManagedThreadId} 客户端处理服务端的消息: {Encoding.UTF8.GetString(data.Data)}");
    });
  
    tcpClient.AddListener(ProtoDefine.ChatRes, data =>
    { 
        var rawData = Encoding.UTF8.GetString(data.Data);
        var chatRes = JsonConvert.DeserializeObject<ChatResponse>(rawData);
        foreach (var msg in chatRes.Messages)
        {
            Console.WriteLine($"【{msg.UserName}】: {msg.Content}");   
        }
    });
    tcpClient.AddListener(ProtoDefine.GetOnlineInfoRes, data =>
    { 
        Console.WriteLine($"线程：{Thread.CurrentThread.ManagedThreadId}【在线玩家信息】 {Encoding.UTF8.GetString(data.Data)}");
    });
}

void PrintTips()
{
    Console.WriteLine("请输入操作：");
    Console.WriteLine("1. 登陆");
    Console.WriteLine("3. 发送消息");
    Console.WriteLine("13. 获取在线玩家数量");
    Console.WriteLine("0. 退出");
}

void DisptachInput(string input)
{
    switch (input)
    {
        case "1":
            handle1();
            break;
        case "3":
            handle3();
            break;
        case "13":
            handle13();
            break;
        default:
            Console.WriteLine("ping");
            input = string.Format("{{\"msg\":\"{0}\"}}", input);
            SendMsg(1, input);
            break;
    }
}

void SendMsg(uint msgId, string input)
{
    var netPack = new UpNetPackage();
    netPack.Data = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(input));
    netPack.MsgId = msgId;
    netPack.AllLength = (uint)netPack.Data.Length + 4 + 4;

    Memory<byte> data = new byte[netPack.AllLength];
    UpPackSerialize.Instance.Serialize(netPack, ref data);
    tcpClient.Connection.Send(data.Span);
}

void handle1()
{
    Console.WriteLine("请输入登陆用户id");
    var input  = Console.ReadLine();
    userId = input;
    LoginRequest user = new LoginRequest();
    user.UserId   = input;
    // user.UserName = input+"昵称";
    user.Password = "111";
    input         = JsonConvert.SerializeObject(user);
    SendMsg(ProtoDefine.LoginReq, input);
}

void handle3()
{
    Console.WriteLine("请输入聊天内容");
    var input = Console.ReadLine();
    ChatRequest chatReq = new ChatRequest();
    chatReq.Message          = new ChatMessage();
    chatReq.Message.UserName = userId;
    chatReq.Message.Content  = input;
    chatReq.Message.MsgType  = 1;
    input                    = JsonConvert.SerializeObject(chatReq);
    SendMsg(ProtoDefine.ChatReq, input);
}

void handle13()
{
    GetOnlineInfoRequest req = new GetOnlineInfoRequest();
    var input           = JsonConvert.SerializeObject(req);
    SendMsg(ProtoDefine.GetOnlineInfoReq, input);
}
