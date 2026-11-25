using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Http3Streams.Handshake;

namespace Http3Streams.Quic;

// One connection. Its owner supplies UDP datagrams and drains the resulting packets under one lock.
internal sealed class QuicEngine : IDisposable
{
    private sealed class PacketSpace(byte[] readSecret, byte[] writeSecret) : IDisposable
    {
        public QuicKeys Reader { get; } = new(readSecret);
        public QuicKeys Writer { get; } = new(writeSecret);
        public CryptoBuffer Crypto { get; } = new();
        public SortedSet<ulong> Received { get; } = [];
        public long Largest = -1;
        public ulong Next;
        public ulong CryptoOffset;
        public bool AckPending;

        public void Dispose()
        {
            Reader.Dispose();
            Writer.Dispose();
        }
    }

    private readonly bool client;
    private readonly QuicTls tls;
    private readonly PacketSpace?[] spaces = new PacketSpace?[3];
    private readonly StreamFlow streams;
    private readonly HashSet<ulong> resetSent = [];
    private readonly HashSet<ulong> stopRequested = [];
    private readonly QuicRecovery recovery = new();
    private byte[] peerId;
    private byte[]? initialPeerId;
    private bool configured;
    private bool addressValidated;
    private long bytesReceived;
    private long bytesSent;

    public byte[] OriginalId
    {
        get;
    }
    public byte[] LocalId { get; } = RandomNumberGenerator.GetBytes(8);
    public bool Established => tls.Complete && !Closed;
    public bool Closed
    {
        get; private set;
    }
    public Exception? Error
    {
        get; private set;
    }
    public event Action<long, long, byte[], bool>? StreamData;
    public event Action<long, long>? StreamAborted;

    private QuicEngine(byte[] originalId, byte[] peerId, X509Certificate2? certificate, string? fingerprint)
    {
        client = certificate is null;
        OriginalId = originalId;
        this.peerId = peerId;
        streams = new StreamFlow(client);
        spaces[0] = CreateSpace(QuicKeys.Initial(originalId, false), QuicKeys.Initial(originalId, true));
        var parameters = TransportParameters.Encode(LocalId, client ? null : originalId);
        if (client)
        {
            tls = QuicTls.Client(fingerprint!, parameters);
            QueueCrypto(0, tls.StartClientHello());
        }
        else
        {
            initialPeerId = peerId;
            tls = QuicTls.Server(certificate!, parameters);
        }
    }

    public static QuicEngine Client(string fingerprint)
    {
        var destination = RandomNumberGenerator.GetBytes(8);
        return new(destination, destination, null, fingerprint);
    }

    public static QuicEngine Server(X509Certificate2 certificate, byte[] firstDatagram)
    {
        var header = QuicPacket.Read(firstDatagram);
        if (firstDatagram.Length < 1200 || header.Level != 0 || header.Destination.Length < 8)
        {
            throw new InvalidDataException("Expected a padded QUIC v1 Initial.");
        }

        return new(header.Destination, header.Source, certificate, null);
    }

    public static byte[]? DestinationId(byte[] datagram)
    {
        try
        {
            return QuicPacket.Read(datagram).Destination;
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    private PacketSpace CreateSpace(byte[] clientSecret, byte[] serverSecret) =>
        client ? new(serverSecret, clientSecret) : new(clientSecret, serverSecret);

    private void QueueCrypto(int level, byte[] flight)
    {
        var space = spaces[level]!;
        for (int offset = 0; offset < flight.Length; offset += 800)
        {
            var bytes = flight[offset..Math.Min(offset + 800, flight.Length)];
            recovery.Add(level, [0x06, .. QuicData.Int(space.CryptoOffset), .. QuicData.Vector(bytes)]);
            space.CryptoOffset += (ulong)bytes.Length;
        }
    }

    private void AdvanceHandshake(int level, byte[] message)
    {
        var wasComplete = tls.Complete;
        var output = tls.Receive(level, message);
        if (level == 0 && tls.ClientHandshakeTraffic.Length > 0)
        {
            spaces[1] ??= CreateSpace(tls.ClientHandshakeTraffic, tls.ServerHandshakeTraffic);
        }

        if (tls.ClientTraffic.Length > 0)
        {
            spaces[2] ??= CreateSpace(tls.ClientTraffic, tls.ServerTraffic);
        }

        if (!configured && tls.PeerParameters is { } parameters)
        {
            streams.Configure(new TransportParameters(parameters, initialPeerId!, client ? OriginalId : null));
            configured = true;
        }

        foreach (var (sendLevel, bytes) in output)
        {
            QueueCrypto(sendLevel, bytes);
        }

        if (!client && !wasComplete && tls.Complete)
        {
            recovery.Add(2, [0x1e]); // HANDSHAKE_DONE
            Discard(1);
        }
    }

    private void Discard(int level)
    {
        recovery.Discard(level);
        spaces[level]?.Dispose();
        spaces[level] = null;
    }

    public void Receive(byte[] datagram)
    {
        bytesReceived += datagram.Length;
        while (!Closed && datagram.Length > 0)
        {
            QuicPacket packet;
            try
            {
                packet = QuicPacket.Read(datagram);
            }
            catch (InvalidDataException)
            {
                return; // A malformed UDP datagram has no authenticated sender.
            }

            datagram = datagram[packet.Bytes.Length..];
            try
            {
                ReceivePacket(packet);
            }
            catch (Exception error) when (error is InvalidDataException or CryptographicException or AuthenticationException)
            {
                Error = error;
                Close(0x101);
            }
        }
    }

    private void ReceivePacket(QuicPacket packet)
    {
        if (packet.Level < 0 || spaces[packet.Level] is not { } space || packet.Level == 2 && !tls.Complete)
        {
            return;
        }

        if (!packet.Destination.AsSpan().SequenceEqual(LocalId)
            && (client || packet.Level != 0 || !packet.Destination.AsSpan().SequenceEqual(OriginalId)))
        {
            return;
        }

        if (packet.Level < 2 && initialPeerId is not null && !packet.Source.AsSpan().SequenceEqual(initialPeerId))
        {
            return;
        }

        DecryptedQuicPacket decoded;
        try
        {
            decoded = space.Reader.Open(packet.Bytes, packet.NumberOffset, ref space.Largest);
        }
        catch (Exception error) when (error is CryptographicException or InvalidDataException)
        {
            return;
        }

        if (initialPeerId is null && packet.Level == 0)
        {
            initialPeerId = peerId = packet.Source;
        }

        if (packet.Level == 1)
        {
            addressValidated = true;
            if (spaces[0] is not null)
            {
                Discard(0);
            }
        }

        if ((decoded.Flags & (packet.Level == 2 ? 0x1c : 0x0c)) != 0)
        {
            throw new InvalidDataException("Unsupported QUIC reserved bits or key phase.");
        }

        if (!space.Received.Add(decoded.Number))
        {
            space.AckPending = true;
            return;
        }

        if (space.Received.Count > 1024)
        {
            space.Received.Remove(space.Received.Min);
        }

        var input = new QuicData(decoded.Data);
        while (!Closed && input.Left > 0)
        {
            var type = input.Int();
            if (packet.Level < 2 && type is not (0 or 1 or 2 or 3 or 6 or 0x1c))
            {
                throw new InvalidDataException("Frame at the wrong QUIC encryption level.");
            }

            space.AckPending |= type is not (0 or 2 or 3 or 0x1c or 0x1d);
            ReadFrame(type, packet.Level, space, input);
        }
    }

    private void ReadFrame(ulong type, int level, PacketSpace space, QuicData input)
    {
        switch (type)
        {
            case 0: // PADDING
            case 1: // PING
                return;
            case 2 or 3:
                recovery.Acknowledge(level, QuicAcks.Read(input, type == 3), space.Next);
                return;
            case 6:
                space.Crypto.Add(input.Int(), input.Vector());
                while (space.Crypto.Message() is { } message)
                {
                    AdvanceHandshake(level, message);
                }
                return;
            case >= 8 and <= 15:
                var id = input.Int();
                var position = (type & 4) != 0 ? input.Int() : 0;
                var bytes = (type & 2) != 0 ? input.Vector() : input.Take(input.Left);
                streams.Receive(id, position + (ulong)bytes.Length);
                StreamData?.Invoke((long)id, (long)position, bytes, (type & 1) != 0);
                return;
            case 4: // RESET_STREAM
                var reset = input.Int();
                var code = input.Int();
                streams.Receive(reset, input.Int());
                StreamAborted?.Invoke((long)reset, (long)code);
                return;
            case 5: // STOP_SENDING
                var stopped = input.Int();
                var reason = input.Int();
                if ((stopped & 2) != 0 && !LocallyInitiated(stopped))
                {
                    throw new InvalidDataException("STOP_SENDING on a receive-only stream.");
                }
                ResetSendSide(stopped, reason);
                StreamAborted?.Invoke((long)stopped, (long)reason);
                return;
            case 0x10:
                streams.GrantConnection(input.Int());
                return;
            case 0x11:
                streams.Grant(input.Int(), input.Int());
                return;
            case 0x12 or 0x13:
                streams.GrantStreams(type == 0x13, input.Int());
                return;
            case 0x14 or 0x16 or 0x17 or 0x19:
                input.Int();
                return;
            case 0x15:
                input.Int();
                input.Int();
                return;
            case 0x18: // NEW_CONNECTION_ID
                var sequence = input.Int();
                var retire = input.Int();
                var cid = input.Take(input.Byte());
                input.Take(16);
                if (cid.Length is 0 or > 20 || retire > sequence)
                {
                    throw new InvalidDataException("Invalid QUIC connection ID.");
                }
                if (retire > 0)
                {
                    peerId = cid;
                }
                return;
            case 0x1a:
                recovery.Add(2, [0x1b, .. input.Take(8)]);
                return;
            case 0x1b:
                input.Take(8);
                return;
            case 0x1c or 0x1d:
                var error = input.Int();
                if (type == 0x1c)
                {
                    input.Int();
                }
                input.Vector();
                if (error is not (0 or 0x100))
                {
                    Error = new IOException($"Peer closed QUIC with error 0x{error:x}.");
                }
                recovery.Clear();
                Closed = true;
                return;
            case 0x1e when client:
                Discard(1);
                return;
            default:
                throw new InvalidDataException($"Unsupported QUIC frame 0x{type:x}.");
        }
    }

    public void QueueStream(long streamId, long offset, ReadOnlyMemory<byte> payload, bool fin)
    {
        ObjectDisposedException.ThrowIf(Closed, this);
        streams.Add(streamId, offset, payload, fin);
    }

    private bool LocallyInitiated(ulong id) => (id & 1) == (client ? 0UL : 1UL);

    private void ResetSendSide(ulong id, ulong errorCode)
    {
        if (resetSent.Add(id))
        {
            ulong finalSize = streams.Stop(id);
            recovery.Add(2, [4, .. QuicData.Int(id), .. QuicData.Int(errorCode), .. QuicData.Int(finalSize)]);
        }
    }

    public void AbortStream(long id, long errorCode)
    {
        if (!Established)
        {
            return;
        }

        ulong streamId = (ulong)id;
        bool bidirectional = (id & 2) == 0;
        if (bidirectional || LocallyInitiated(streamId))
        {
            ResetSendSide(streamId, (ulong)errorCode);
        }
        if ((bidirectional || !LocallyInitiated(streamId)) && stopRequested.Add(streamId))
        {
            recovery.Add(2, [5, .. QuicData.Int(streamId), .. QuicData.Int((ulong)errorCode)]);
        }
    }

    public List<byte[]> Flush()
    {
        if (Established)
        {
            streams.Schedule(recovery);
        }
        var output = new List<byte[]>();
        var now = Environment.TickCount64;
        for (int level = 0; level < spaces.Length; level++)
        {
            if (spaces[level] is not { } space)
            {
                continue;
            }

            while (true)
            {
                var candidate = recovery.Ready(level, now).FirstOrDefault();
                var ack = space.AckPending ? QuicAcks.Encode(space.Received) : [];
                if (candidate is null && ack.Length == 0)
                {
                    break;
                }

                var include = candidate is not null && ack.Length + candidate.Bytes.Length <= 1100;
                byte[] plain = [.. ack, .. (include ? candidate!.Bytes : [])];
                byte[] Encode(byte[] frame) => level == 2
                    ? QuicPacket.Short(peerId, space.Next, frame, space.Writer)
                    : QuicPacket.Long(level, peerId, LocalId, space.Next, frame, space.Writer);
                var packet = Encode(plain);
                if (include && !recovery.CanSend(candidate!, packet.Length))
                {
                    if (ack.Length == 0)
                    {
                        break;
                    }
                    include = false;
                    packet = Encode(ack);
                }

                if (!client && !addressValidated && bytesSent + packet.Length > bytesReceived * 3)
                {
                    break;
                }

                if (include)
                {
                    recovery.Record(candidate!, space.Next, packet.Length, now);
                }

                space.Next++;
                bytesSent += packet.Length;
                space.AckPending = false;
                output.Add(packet);
            }
        }

        return output;
    }

    public void Close(long errorCode = 0x100)
    {
        if (Closed)
        {
            return;
        }

        recovery.Clear();
        var level = tls.Complete ? 2 : spaces[1] is not null ? 1 : 0;
        byte[] frame = level == 2 ? [0x1d, .. QuicData.Int((ulong)errorCode), 0] : [0x1c, 0x0c, 0, 0];
        recovery.Add(level, frame, false);
        Closed = true;
    }

    public void Dispose()
    {
        tls.Dispose();
        foreach (var space in spaces)
        {
            space?.Dispose();
        }
    }
}
