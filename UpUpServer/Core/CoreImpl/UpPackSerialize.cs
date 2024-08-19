namespace UpUpServer;

using System.Buffers;
using System.Runtime.CompilerServices;

public class UpPackSerialize : SingleClass<UpPackSerialize>, IPackSerialize
{
    public UpPackSerialize()
    {
    }

    public uint GetHeadLen()
    {
        // len + msgid + data
        return 8;
    }

    public void Serialize(INetPackage msg, ref Memory<byte> data)
    {
        Span<byte> spanData = data.Span;
        Unsafe.As<byte, uint>(ref spanData[0]) = msg.AllLength;
        Unsafe.As<byte, uint>(ref spanData[4]) = msg.MsgId;
        // BitConverter.TryWriteBytes(data, msg.AllLength);
        // BitConverter.TryWriteBytes(data.Slice(4), msg.MsgId);
        msg.Data.CopyTo(spanData.Slice(8));
    }

    public INetPackage Deserialize(ReadOnlySequence<byte> data)
    {
        var headLen = GetHeadLen();

        if (data.Length < headLen)
        {
            Log.Error($"data head length is not enough, need {headLen}, but {data.Length}");
            return null;
        }

        Span<byte> firstFourBytes = stackalloc byte[4]; // len
        Span<byte> secondFourBytes = stackalloc byte[4]; // msgid
        data.Slice(data.Start, 4).CopyTo(firstFourBytes);
        data.Slice(4, 4).CopyTo(secondFourBytes);
        uint msgLen;
        uint msgId;
        msgLen = Unsafe.ReadUnaligned<uint>(ref firstFourBytes[0]);
        msgId = Unsafe.ReadUnaligned<uint>(ref secondFourBytes[0]);
        // BitConverter.ToUInt32(firstFourBytes);

        if (data.Length < msgLen)
        {
            Log.Error($"data length is not enough, need {msgLen}, but {data.Length}");
            return null;
        }

        var msgBytes = data.Slice(headLen, (int)msgLen - headLen);

        var msg = new UpNetPackage();
        msg.AllLength = msgLen;
        msg.MsgId = msgId;
        msg.Data = msgBytes;

        return msg;
    }
}