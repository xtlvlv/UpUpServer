using MongoDB.Driver;
using UpUpServer;

[AutoRouter(ProtoDefine.ChatReq)]
public class ChatRouter: JsonRpcRouter<ChatRequest, NoResponse>
{
    ChatSystem? _chatSystem;
    public override async Task Handle(IRequest request)
    {
        await base.Handle(request);
        if (_chatSystem is null)
        {
            _chatSystem = Program.Server.SysManager.GetSystem<ChatSystem>();
            if (_chatSystem is null)
            {
                Log.Error("ChatSystem is null");
                return;
            }
        }
        _chatSystem.AddChatRequest(request);
    }
}