namespace UpUpServer;

public interface IMsgManager
{
    IServer Server { get; set; }
    Dictionary<uint, IRouter> Routers { get; }
    void StartWorkPool(IServer server);
    void RegisterRouter(uint msgId, IRouter router);
    void UnRegisterRouter(uint msgId);
    Task HandleMsg(uint msgId, IRequest request);
    void AddRequest(IRequest request);
    void AddResponse(IRequest request);
    void AddPackResponse(IRequest request, uint msgId);


}