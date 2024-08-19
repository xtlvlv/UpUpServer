namespace UpUpServer;
using System.Buffers;

public interface IPackSerialize
{
    uint GetHeadLen();
    void Serialize(INetPackage msg, ref Memory<byte> data);
    INetPackage Deserialize(ReadOnlySequence<byte> data);
}