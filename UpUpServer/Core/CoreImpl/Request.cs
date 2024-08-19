using System.Buffers;

namespace UpUpServer;

public class Request : IRequest
{
    public IConnection Connection { get; set; }
    public INetPackage NetPackage { get; set; }
    public IProtocol? Req { get; set; }
    public IProtocol Res { get; set; }
    public INetPackage ResNetPackage { get; set; }

    public Request(IConnection connection, INetPackage netPackage)
    {
        Connection = connection;
        NetPackage = netPackage;
    }
}