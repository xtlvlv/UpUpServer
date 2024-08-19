using UpUpServer;

public class LoginRequest : IProtocol
{
    public string UserId;
    public string Password;
}

public class LoginResponse : IProtocol
{
    public int Status;

    // 数据及数据版本
    public string Data;
    public ulong  DataVersion;
    public ulong  Uid;

}

