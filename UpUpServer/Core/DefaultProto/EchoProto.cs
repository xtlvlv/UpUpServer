namespace UpUpServer;

public class EchoReq : IProtocol
{
    public string Msg { get; set; }
}

public class EchoRes : IProtocol
{
    public string Msg { get; set; }
}

public class NoResponse: IProtocol
{
}