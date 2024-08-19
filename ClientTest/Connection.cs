using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

public class Connection
{
    public event Action? OnConnected;
    public event Action<ReadOnlySequence<byte>>? OnReceived;
    public event Action<string>? OnClose;

    public readonly IPAddress Ip;
    public readonly int Port;
    internal readonly Socket Socket;
    private readonly Pipe _pipe;
    private volatile bool _isDispose;
    private readonly int _bufferSize = 10 * 1024;

    public Connection(string ip, int port)
    {
        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket.NoDelay = true;
        // Socket.ReceiveTimeout = 1000 * 60 * 5; //5分钟没收到东西就算超时
        Ip = IPAddress.Parse(ip);
        Port = port;
        _pipe = new Pipe();
    }

    public void Connect()
    {
        if (Socket.Connected) return;
        Socket.Connect(Ip, Port);
        Start();
    }

    internal async void Start()
    {
        SetSocket();
        Task writing = FillPipeAsync(Socket, _pipe.Writer);
        Task reading = ReadPipeAsync(_pipe.Reader);
        _ = Task.WhenAll(reading, writing);
        OnConnected?.Invoke();
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
        catch (SocketException)
        {
            Close("connection has been closed");
        }
        catch (ObjectDisposedException)
        {
            Close("connection has been closed");
        }
        catch (Exception ex)
        {
            Close($"{ex.Message}\n{ex.StackTrace}");
        }

        // By completing PipeWriter, tell the PipeReader that there's no more data coming.
        await writer.CompleteAsync();
        Close("connection has been closed");
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
                // Process callback
                try
                {
                    OnReceived?.Invoke(packet);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            // Tell the PipeReader how much of the buffer has been consumed.
            reader.AdvanceTo(buffer.Start);

            // Stop reading if there's no more data coming.
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
        //     var b = buffer.FirstSpan[0];
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
    
    public void Send(Span<byte> buffer)
    {
        try
        {
            if (!_isDispose)
            {
                Socket.Send(buffer);
            }
        }
        catch
        {
            Close("connection has been closed");
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
            Close("connection has been closed");
        }
    }

    public void Close(string msg = "closed manually")
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

            OnClose?.Invoke(msg);
        }
    }
}