namespace UpUpServer;

public interface IRequest
{
    IConnection Connection { get; set; }
    INetPackage NetPackage { get; set; }
    IProtocol? Req { get; set; }
    IProtocol Res { get; set; }
    INetPackage ResNetPackage { get; set; }

}