namespace UpUpServer;

public interface IServer
{
    IMsgManager MsgManager { get; set; }
    IConnManager ConnManager { get; set; }

    bool IsRunning { get; set; }

    void Start();
    void Stop();
    void AddRouter(uint msgId, IRouter router);
}