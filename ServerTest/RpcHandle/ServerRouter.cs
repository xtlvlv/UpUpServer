using MongoDB.Driver;
using UpUpServer;

[AutoRouter(ProtoDefine.GetOnlineInfoReq)]
public class GetOnlineInfoRouter: JsonRpcRouter<GetOnlineInfoRequest, GetOnlineInfoResponse>
{

    public override async Task Handle(IRequest request)
    {
        await base.Handle(request);
        var req = (GetOnlineInfoRequest)request.Req!;
        var res = (GetOnlineInfoResponse)request.Res;
        
        res.OnlinePlayerCount = Program.Server.ConnManager.GetOnlinePlayerCount();
    }
}
