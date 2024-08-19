using System.Buffers;

public class UpNetPackage
{
    public uint AllLength { get; set; }
    public uint MsgId { get; set; }
    public ReadOnlySequence<byte> Data { get; set; }
}