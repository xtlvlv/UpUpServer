using System.Buffers;

namespace UpUpServer;

public interface INetPackage
{
    uint AllLength { get; set; }
    uint MsgId { get; set; }
    ReadOnlySequence<byte> Data { get; set; }
}