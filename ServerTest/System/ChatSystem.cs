using System.Collections.Concurrent;
using UpUpServer;

/// <summary>
/// 聊天系统
/// </summary>
[AutoSystem]
public class ChatSystem: ISystem
{
    private ConcurrentQueue<IRequest> _chatMessages;    // 聊天消息缓存
    private bool isUpdating = false;
    public void Init()
    {
        Log.Info("ChatSystem init");
        _chatMessages = new ConcurrentQueue<IRequest>();
    }

    public void Update()
    {
        if (isUpdating)
        {
            return;
        }

        if (_chatMessages.Count<=0)
        {
            return;
        }
        isUpdating = true;
        List<ChatMessage> chatList = new List<ChatMessage>();
        List<IRequest> chatRequests = new List<IRequest>();

        // 聊天消息合并
        while (_chatMessages.TryDequeue(out var chatRequest))
        {
            chatRequests.Add(chatRequest);
            var chatMessage = (chatRequest.Req as ChatRequest)?.Message;
            if (chatMessage != null)
            {
                chatList.Add(chatMessage);
                Log.Warning($"{chatMessage.UserName} say: {chatMessage.Content}");
            }
        }

        var res = new ChatResponse();
        res.Messages = chatList;
        // 通知所有客户端
        Program.Server.ConnManager.Connections.Keys.ToList().ForEach(connKey =>
        {
            var conn = Program.Server.ConnManager.Connections[connKey];
            Request request = new Request(conn, null);
            request.Res = res;
            Program.Server.MsgManager.AddPackResponse(request, ProtoDefine.ChatRes);
        });
        
        isUpdating = false;
    }

    public void Dispose()
    {
    }
    
    public void AddChatRequest(IRequest request)
    {
        _chatMessages.Enqueue(request);
    }
    
}