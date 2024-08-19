using System.Collections.Concurrent;

namespace UpUpServer;

public class ConnManager: IConnManager
{
    public ConcurrentDictionary<uint, IConnection> Connections { get; } = new ConcurrentDictionary<uint, IConnection>();
    public void AddConnection(IConnection connection)
    {
        Connections.TryAdd(connection.Id, connection);
    }

    public void RemoveConnection(uint connId)
    {
        Connections.TryRemove(connId, out var conn);
    }

    public IConnection? GetConnection(uint connId)
    {
        Connections.TryGetValue(connId, out var conn);
        return conn;
    }

    public void CheckOnline()
    {
        var keys = Connections.Keys.ToArray();
        foreach (var cid in keys)
        {
            var conn = GetConnection(cid);
            if (conn == null)
            {
                RemoveConnection(cid);
                continue;
            }
            else
            {
                if (!conn.IsConnected)
                {
                    conn.Close();
                    RemoveConnection(cid);
                    Log.Info($"ConnManager kick conn {cid}");
                }
            }
        }
    }

    public int GetOnlinePlayerCount()
    {
        var count = Connections.Count;
        return count;
    }
    public void Release()
    {
        
    }
}