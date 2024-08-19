
using UpUpServer;

// 获取服务器信息
public class GetOnlineInfoRequest : IProtocol
{
}

public class GetOnlineInfoResponse : IProtocol
{
    public int OnlinePlayerCount;   // 在线玩家数量
}
