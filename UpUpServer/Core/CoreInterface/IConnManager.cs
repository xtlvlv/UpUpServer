using System.Collections.Concurrent;

namespace UpUpServer;

public interface IConnManager
{
    ConcurrentDictionary<uint, IConnection> Connections { get; }
    
    void         AddConnection(IConnection connection);
    void         RemoveConnection(uint     connId);
    IConnection? GetConnection(uint        connId);
    void         CheckOnline();
    int          GetOnlinePlayerCount();

    void Release();
}