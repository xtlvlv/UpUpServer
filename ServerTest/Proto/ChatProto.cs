
using UpUpServer;

public class ChatRequest : IProtocol
{
    public ChatMessage Message;
}

public class ChatResponse : IProtocol
{
    public List<ChatMessage> Messages;
}

public class ChatMessage : IProtocol
{
    public int    Channel;  // 频道
    public int    MsgType;  // 消息类型
    public string UserName;
    public string Content;
}

public class ChatChannelDefine
{
    public const int World = 1;
    public const int Team  = 2;
}

public class ChatMsgTypeDefine
{
    public const int Text = 1;  // 支持表情
    public const int Equip  = 2;
}

