namespace UpUpServer;

public interface IConnection
{
    uint Id { get; set; }
    string UserId { get; }

    IMsgManager MsgManager { get; set; }

    bool IsConnected { get; }

    void SetUserId(string userId);
    void Start();
    void Close();
    void Send(Memory<byte> buffer);
    ValueTask SendAsync(Memory<byte> data);
}