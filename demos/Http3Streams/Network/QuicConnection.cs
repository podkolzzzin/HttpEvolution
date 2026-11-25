using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading.Channels;
using Http3Streams.Quic;

namespace Http3Streams.Network;

internal sealed class QuicConnection : IAsyncDisposable
{
    private readonly QuicEngine engine;
    private readonly Action<byte[]> send;
    private readonly bool client;
    private readonly Dictionary<long, QuicStream> streams = [];
    private readonly Channel<QuicStream> inbound = Channel.CreateUnbounded<QuicStream>();
    private readonly TaskCompletionSource handshake = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Action<QuicConnection>? accepted;
    private CancellationTokenSource? clientLifetime;
    private Task clientPump = Task.CompletedTask;
    private long lastReceived = Environment.TickCount64;
    private long nextBidirectional;
    private long nextUnidirectional;
    private IOException? failure;

    // The peer can open the advertised number of streams in each directionality class.
    internal const int MaxInboundStreams = 2 * TransportParameters.StreamCount;
    internal string Id => Convert.ToHexString(engine.LocalId);
    internal object Gate { get; } = new();
    internal bool Ended
    {
        get
        {
            lock (Gate)
            {
                return failure is not null;
            }
        }
    }

    internal QuicConnection(QuicEngine engine, bool client, Action<byte[]> send, Action<QuicConnection>? accepted = null)
    {
        this.engine = engine;
        this.client = client;
        this.send = send;
        this.accepted = accepted;
        nextBidirectional = client ? 0 : 1;
        nextUnidirectional = nextBidirectional + 2;
        engine.StreamData += (id, offset, bytes, fin) => PeerStream(id).Receive(offset, bytes, fin);
        engine.StreamAborted += (id, code) => PeerStream(id).Fail(new QuicStreamAbortedException(id, code));
    }

    public static async Task<QuicConnection> ConnectAsync(IPEndPoint endpoint, string expectedFingerprint, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var engine = QuicEngine.Client(expectedFingerprint);
        var socket = new UdpClient(endpoint.AddressFamily);
        var connection = new QuicConnection(engine, true, bytes => socket.Send(bytes, bytes.Length));
        try
        {
            socket.Connect(endpoint);
            connection.clientLifetime = new CancellationTokenSource();
            connection.clientPump = connection.PumpClientAsync(socket, connection.clientLifetime.Token);
            connection.Tick();
            await connection.handshake.Task.WaitAsync(token).ConfigureAwait(false);
            lock (connection.Gate)
            {
                if (connection.failure?.InnerException is AuthenticationException authentication)
                {
                    throw authentication;
                }
                connection.EnsureOpen();
            }
            return connection;
        }
        catch
        {
            await connection.DisposeAsync().ConfigureAwait(false);
            socket.Dispose();
            throw;
        }
    }

    private async Task PumpClientAsync(UdpClient socket, CancellationToken token)
    {
        try
        {
            await DatagramPump.RunAsync(socket, packet => Receive(packet.Buffer), Tick, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (SocketException error)
        {
            Disconnect(new IOException("UDP connection failed.", error));
        }
        finally
        {
            socket.Dispose();
        }
    }

    internal void Receive(byte[] datagram)
    {
        lock (Gate)
        {
            if (failure is not null)
            {
                return;
            }
            lastReceived = Environment.TickCount64;
            try
            {
                engine.Receive(datagram);
                Service();
            }
            catch (Exception error) when (IsTransportError(error))
            {
                Disconnect(new IOException("Invalid QUIC exchange.", error));
            }
        }
    }

    internal void Tick()
    {
        lock (Gate)
        {
            if (failure is not null)
            {
                return;
            }
            long timeout = engine.Established ? 30_000 : 8_000;
            if (Environment.TickCount64 - lastReceived >= timeout)
            {
                Disconnect(new IOException("QUIC peer timed out."));
                return;
            }
            try
            {
                Service();
            }
            catch (Exception error) when (IsTransportError(error))
            {
                Disconnect(new IOException("QUIC transport failed.", error));
            }
        }
    }

    private static bool IsTransportError(Exception error) =>
        error is IOException or InvalidDataException or SocketException or AuthenticationException
            or System.Security.Cryptography.CryptographicException;

    private void Service()
    {
        SendPending();
        if (engine.Closed)
        {
            Disconnect(new IOException("QUIC connection closed.", engine.Error));
        }
        else if (engine.Established && handshake.TrySetResult())
        {
            accepted?.Invoke(this);
        }
    }

    private void SendPending()
    {
        foreach (byte[] datagram in engine.Flush())
        {
            send(datagram);
        }
    }

    public QuicStream OpenStream(bool bidirectional)
    {
        lock (Gate)
        {
            EnsureOpen();
            ref long next = ref (bidirectional ? ref nextBidirectional : ref nextUnidirectional);
            if (next / 4 >= TransportParameters.StreamCount)
            {
                throw new IOException("Stream limit reached for this connection.");
            }
            var stream = new QuicStream(next, true, this);
            streams.Add(next, stream);
            next += 4;
            return stream;
        }
    }

    private QuicStream PeerStream(long id)
    {
        if (streams.TryGetValue(id, out var stream))
        {
            return stream;
        }
        if (id < 0 || id / 4 >= TransportParameters.StreamCount || (id & 1) == (client ? 0 : 1))
        {
            throw new InvalidDataException("Peer referenced an invalid stream ID.");
        }
        stream = new(id, false, this);
        streams.Add(id, stream);
        inbound.Writer.TryWrite(stream);
        return stream;
    }

    public async Task<QuicStream> AcceptStreamAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await inbound.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (ChannelClosedException error)
        {
            throw new IOException("QUIC connection closed.", error.InnerException);
        }
    }

    internal void QueueData(long id, long offset, ReadOnlyMemory<byte> data, bool fin)
    {
        EnsureOpen();
        if (offset + data.Length > TransportParameters.StreamWindow)
        {
            throw new IOException("This demo supports up to 256 KiB per stream.");
        }
        if (!data.IsEmpty || fin)
        {
            engine.QueueStream(id, offset, data, fin);
        }
    }

    internal void AbortStream(long id, long code)
    {
        if (failure is null)
        {
            engine.AbortStream(id, code);
        }
    }

    private void EnsureOpen()
    {
        if (failure is not null)
        {
            throw failure;
        }
    }

    internal void Disconnect(IOException reason)
    {
        lock (Gate)
        {
            if (failure is not null)
            {
                return;
            }
            failure = reason;
            handshake.TrySetResult();
            inbound.Writer.TryComplete(reason);
            foreach (var stream in streams.Values)
            {
                stream.Fail(reason);
            }
            engine.Dispose();
            clientLifetime?.Cancel();
        }
    }

    public async ValueTask CloseAsync(long errorCode = 0x100)
    {
        lock (Gate)
        {
            if (failure is null)
            {
                try
                {
                    engine.Close(errorCode);
                    SendPending();
                }
                catch (SocketException)
                {
                    // The local endpoint has already lost its peer.
                }
                finally
                {
                    Disconnect(new IOException("QUIC connection closed locally."));
                }
            }
        }
        await clientPump.ConfigureAwait(false);
        lock (Gate)
        {
            clientLifetime?.Dispose();
            clientLifetime = null;
        }
    }

    public ValueTask DisposeAsync() => CloseAsync();
}
