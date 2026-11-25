using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;
using Http3Streams.Quic;

namespace Http3Streams.Network;

internal sealed class QuicListener : IAsyncDisposable
{
    private readonly UdpClient socket;
    private readonly X509Certificate2 certificate;
    private readonly CancellationTokenSource lifetime = new();
    private readonly Channel<QuicConnection> accepted = Channel.CreateBounded<QuicConnection>(64);
    private readonly Dictionary<(IPEndPoint Peer, string Id), QuicConnection> routes = [];
    private readonly HashSet<QuicConnection> sessions = [];
    private readonly object sendGate = new();
    private readonly Task pump;

    public QuicListener(IPEndPoint endpoint, X509Certificate2 certificate)
    {
        this.certificate = certificate;
        socket = new UdpClient(endpoint);
        LocalEndPoint = (IPEndPoint)socket.Client.LocalEndPoint!;
        pump = ListenAsync();
    }

    public IPEndPoint LocalEndPoint
    {
        get;
    }

    public async Task<QuicConnection> AcceptAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await accepted.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (ChannelClosedException error)
        {
            throw new IOException("QUIC listener closed.", error.InnerException);
        }
    }

    private async Task ListenAsync()
    {
        try
        {
            await DatagramPump.RunAsync(socket, Dispatch, Tick, lifetime.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
        }
        catch (SocketException error)
        {
            accepted.Writer.TryComplete(error);
        }
        finally
        {
            accepted.Writer.TryComplete();
            foreach (var session in sessions)
            {
                await session.CloseAsync().ConfigureAwait(false);
            }
            socket.Dispose();
        }
    }

    private void Dispatch(UdpReceiveResult datagram)
    {
        byte[]? destinationId = QuicEngine.DestinationId(datagram.Buffer);
        if (destinationId is null)
        {
            return;
        }
        var peer = datagram.RemoteEndPoint;
        var key = (peer, Convert.ToHexString(destinationId));
        if (!routes.TryGetValue(key, out var session))
        {
            // Only a QUIC v1 Initial of at least 1200 bytes can create a session.
            if (sessions.Count >= 64 || datagram.Buffer.Length < 1200 || (datagram.Buffer[0] & 0xf0) != 0xc0)
            {
                return;
            }
            QuicEngine engine;
            try
            {
                engine = QuicEngine.Server(certificate, datagram.Buffer);
            }
            catch (InvalidDataException)
            {
                return;
            }
            session = new QuicConnection(engine, false, bytes => Send(peer, bytes), Publish);
            sessions.Add(session);
            routes.Add(key, session);
            routes[(peer, Convert.ToHexString(engine.LocalId))] = session;
        }
        session.Receive(datagram.Buffer);
    }

    private void Publish(QuicConnection session)
    {
        if (!accepted.Writer.TryWrite(session))
        {
            session.Disconnect(new IOException("QUIC accept queue is full."));
        }
    }

    private void Send(IPEndPoint endpoint, byte[] bytes)
    {
        lock (sendGate)
        {
            socket.Send(bytes, bytes.Length, endpoint);
        }
    }

    private void Tick()
    {
        foreach (var session in sessions)
        {
            session.Tick();
        }
        if (sessions.RemoveWhere(session => session.Ended) != 0)
        {
            foreach (var route in routes.Where(route => route.Value.Ended).ToArray())
            {
                routes.Remove(route.Key);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await lifetime.CancelAsync().ConfigureAwait(false);
        await pump.ConfigureAwait(false);
        lifetime.Dispose();
    }
}
