namespace UpUpServer;

public class EchoRouter : JsonRpcRouter<EchoReq, EchoRes>
{
    public override Task Handle(IRequest request)
    {
        if (ErrorCode != 0)
        {
            return Task.CompletedTask;
        }
        base.Handle(request);
        var req = (EchoReq)request.Req!;
        var res = (EchoRes)request.Res;

        res.Msg = req.Msg;
        
        return Task.CompletedTask;
    }
}