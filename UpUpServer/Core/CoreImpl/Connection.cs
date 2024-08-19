using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace UpUpServer;

public class Connection : IConnection
{
    readonly Socket Socket;
    public IMsgManager MsgManager { get; set; }

    public event Action? OnConnected;
    public event Action<ReadOnlySequence<byte>>? OnReceived;
    public event Action? OnClose;

    public readonly IPAddress Ip;
    public readonly int Port;

    private readonly Pipe _pipe;

    private volatile bool _isDispose;

    //默认10K的缓冲区空间
    private readonly int _bufferSize = 10 * 1024;
    
    public uint Id { get; set; } = 0;
    public string UserId { get; set; } = "";

    public bool IsConnected
    {
        get => Socket.Connected;
    }

    internal Connection(uint id, Socket socket, IMsgManager msgManager)
    {
        Id = id;
        Socket = socket;
        IPEndPoint? remoteIpEndPoint = socket.RemoteEndPoint as IPEndPoint;
        Ip = remoteIpEndPoint?.Address!;
        Port = remoteIpEndPoint?.Port ?? 0;
        _pipe = new Pipe();
        MsgManager = msgManager;
    }

    public async void Start()
    {
        SetSocket();
        Task writing = FillPipeAsync(Socket, _pipe.Writer);
        Task reading = ReadPipeAsync(_pipe.Reader);
        _ = Task.WhenAll(reading, writing);
        OnConnected?.Invoke();
    }
    
    public void SetUserId(string userId)
    {
        UserId = userId;
    }

    private void SetSocket()
    {
        _isDispose = false;
        Socket.ReceiveBufferSize = _bufferSize;
        Socket.SendBufferSize = _bufferSize;
    }

    /// <summary>
    /// Read from socket and write to pipe
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="writer"></param>
    async Task FillPipeAsync(Socket socket, PipeWriter writer)
    {
        try
        {
            while (!_isDispose && Socket.Connected)
            {
                // Allocate at least _bufferSize bytes from the PipeWriter.
                Memory<byte> memory = writer.GetMemory(_bufferSize);
                try
                {
                    int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    // Tell the PipeWriter how much was read from the Socket.
                    writer.Advance(bytesRead);
                }
                catch (SocketException e)
                {
                    //connection ended
                    if (e.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    break;
                }

                // Make the data available to the PipeReader.
                FlushResult result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }
        }
        catch (SocketException e)
        {
            Log.Warning($"SocketException closed, ip={Ip} exception={e.Message}");
            Close();
        }
        catch (ObjectDisposedException e)
        {
            Log.Warning($"ObjectDisposedException closed, ip={Ip} exception={e.Message}");
            Close();
        }
        catch (Exception e)
        {
            Log.Warning($"Exception closed, ip={Ip} exception={e.Message}");
            Close();
        }

        // By completing PipeWriter, tell the PipeReader that there's no more data coming.
        await writer.CompleteAsync();
        Log.Info($"client close connection, ip={Ip}");
        Close(); // "connection has been closed"
    }

    /// <summary>
    /// Read from pipe and process
    /// </summary>
    /// <param name="reader"></param>
    async Task ReadPipeAsync(PipeReader reader)
    {
        while (true)
        {
            ReadResult result = await reader.ReadAsync();
            ReadOnlySequence<byte> buffer = result.Buffer;
            while (HandleStickyPack(ref buffer, out ReadOnlySequence<byte> packet))
            {
                try
                {
                    Request request = new Request(this, UpPackSerialize.Instance.Deserialize(packet));
                    MsgManager.AddRequest(request);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    this.Close();
                }
            }

            reader.AdvanceTo(buffer.Start);
            if (result.IsCompleted)
            {
                break;
            }
        }

        // Mark the PipeReader as complete.
        await reader.CompleteAsync();
    }

    bool HandleStickyPack(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> packet)
    {
        //first 4 bytes is a uint represents length of packet
        if (buffer.Length < UpPackSerialize.Instance.GetHeadLen())
        {
            packet = default;
            return false;
        }

        uint length;

        // if (buffer.IsSingleSegment) 
        // {
        //     var b = buffer.FirstSpan[0]; 只会读取第一个byte，读的长度有问题
        //     length = Unsafe.ReadUnaligned<uint>(ref b);
        // }
        // else
        {
            Span<byte> firstFourBytes = stackalloc byte[4];
            buffer.Slice(buffer.Start, 4).CopyTo(firstFourBytes);
            length = Unsafe.ReadUnaligned<uint>(ref firstFourBytes[0]);
        }

        if (buffer.Length < length)
        {
            packet = default;
            return false;
        }

        packet = buffer.Slice(buffer.Start, length);
        buffer = buffer.Slice(length);
        return true;
    }

    bool TryParsePacket(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> packet)
    {
        //first 4 bytes is a uint represents length of packet
        if (buffer.Length < 4)
        {
            packet = default;
            return false;
        }

        uint length;
        //read length uint (this length includes the length of the length, 4 bytes)
        if (buffer.IsSingleSegment)
        {
            var b = buffer.FirstSpan[0];
            length = Unsafe.ReadUnaligned<uint>(ref b);
        }
        else
        {
            Span<byte> firstFourBytes = stackalloc byte[4];
            buffer.Slice(buffer.Start, 4).CopyTo(firstFourBytes);
            length = Unsafe.ReadUnaligned<uint>(ref firstFourBytes[0]);
        }

        // Read the packet
        if (buffer.Length < length)
        {
            packet = default;
            return false;
        }

        packet = buffer.Slice(4, length - 4);
        buffer = buffer.Slice(length);
        return true;
    }

    public void Send(Memory<byte> data)
    {
        try
        {
            if (!_isDispose)
            {
                Socket.Send(data.Span);
            }
        }
        catch
        {
            Close(); // "connection has been closed"
        }
    }

    public async ValueTask SendAsync(Memory<byte> data)
    {
        try
        {
            if (!_isDispose)
            {
                await Socket.SendAsync(data, SocketFlags.None);
            }
        }
        catch
        {
            Close(); // "connection has been closed"
        }
    }

    public void Close()
    {
        if (!_isDispose)
        {
            _isDispose = true;
            try
            {
                try
                {
                    Socket.Close();
                }
                catch
                {
                    //ignore
                }

                Socket.Dispose();
                GC.SuppressFinalize(this);
            }
            catch (Exception)
            {
                //ignore
            }

            OnClose?.Invoke();
        }
    }
}