using System.Buffers;

namespace UpUpServer;

public class UpNetPackage : INetPackage
{
    public uint AllLength { get; set; }
    public uint MsgId { get; set; }
    public ReadOnlySequence<byte> Data { get; set; }
}