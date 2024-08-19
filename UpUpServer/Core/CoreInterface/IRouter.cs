namespace UpUpServer;

public interface IRouter
{
    int ErrorCode { get; set; }
    IMsgManager MsgManager { get; set; }
    void PreHandle(IRequest request);
    Task Handle(IRequest request);
    void PostHandle(IRequest request);
}