using System.Net;
using System.Security.Cryptography;

sealed class QuicSession : IDisposable
{
    sealed class Space(byte[] client, byte[] server) : IDisposable
    {
        public readonly QuicKeys Read = new(client), Write = new(server);
        public readonly QuicBuffer Crypto = new();
        public readonly SortedSet<ulong> Received = [];
        public readonly Queue<byte[]> Pending = [];
        public long Largest = -1;
        public ulong Next, CryptoOffset;
        public bool WantAck;
        public void Dispose() { Read.Dispose(); Write.Dispose(); }
    }
    readonly Space?[] spaces = new Space?[3];
    readonly TlsHandshake tls = new("h3");
    QuicStreams? streams;
    readonly byte[] original;
    byte[] peerId;
    long received, sent;
    bool validated;
    public byte[] Id { get; } = RandomNumberGenerator.GetBytes(8);
    public IPEndPoint Peer { get; }
    public long LastSeen { get; private set; } = Environment.TickCount64;
    public bool Closed { get; private set; }
    public QuicSession(IPEndPoint peer, byte[] destination, byte[] source)
    {
        Peer = peer; original = destination; peerId = source;
        spaces[0] = new(QuicKeys.Initial(original, false), QuicKeys.Initial(original, true));
    }
    void Crypto(int level, byte[] message)
    {
        var space = spaces[level]!;
        for (var offset = 0; offset < message.Length; offset += 800)
        {
            var part = message[offset..Math.Min(message.Length, offset + 800)];
            space.Pending.Enqueue([6, .. QuicData.Int(space.CryptoOffset), .. QuicData.Vector(part)]);
            space.CryptoOffset += (ulong)part.Length;
        }
    }
    void Handshake(int level, byte[] message)
    {
        if (level == 0)
        {
            Crypto(0, tls.AcceptHello(message));
            if (tls.HandshakeSecret.Length == 0) return;
            if (!tls.PeerExtensions.TryGetValue(57, out var parameters)) throw new InvalidDataException("Missing QUIC parameters.");
            var limits = new QuicParameters(parameters); limits.ValidateClient(peerId);
            streams = new(limits);
            spaces[1] = new(tls.ClientHandshakeTraffic, tls.ServerHandshakeTraffic);
            Crypto(1, tls.ServerFlight(QuicParameters.Server(original, Id)).SelectMany(bytes => bytes).ToArray());
            spaces[2] = new(tls.ClientTraffic, tls.ServerTraffic);
        }
        else if (level == 1)
        {
            tls.VerifyFinished(message); spaces[2]!.Pending.Enqueue([0x1e]);
            Console.WriteLine("QUIC handshake complete; ALPN h3.");
        }
        else throw new InvalidDataException("Unexpected CRYPTO level.");
    }
    public void Receive(QuicPacket packet)
    {
        if (Closed || packet.Level < 0 || spaces[packet.Level] is not { } space) return;
        (byte Flags, ulong Number, byte[] Data) decoded;
        try { decoded = space.Read.Open(packet.Bytes, packet.NumberOffset, ref space.Largest); }
        catch (Exception error) when (error is CryptographicException or InvalidDataException) { return; }
        received += packet.Bytes.Length; LastSeen = Environment.TickCount64;
        if (packet.Level == 1) validated = true;
        if ((decoded.Flags & (packet.Level == 2 ? 0x1c : 0x0c)) != 0) throw new InvalidDataException("Invalid reserved/key-phase bits.");
        if (!space.Received.Add(decoded.Number)) { space.WantAck = true; return; }
        if (space.Received.Count > 1024) space.Received.Remove(space.Received.Min);
        var input = new QuicData(decoded.Data);
        while (input.Left > 0)
        {
            var type = input.Int();
            if (packet.Level < 2 && type is not (0 or 1 or 2 or 3 or 6 or 0x1c)) throw new InvalidDataException("Frame at the wrong encryption level.");
            if (type is not (0 or 2 or 3 or 0x1c or 0x1d)) space.WantAck = true;
            switch (type)
            {
                case 0: case 1: break;
                case 2: case 3: QuicAcks.Read(input, type == 3); break;
                case 6:
                    var offset = input.Int(); space.Crypto.Add(offset, input.Vector());
                    while (space.Crypto.Message() is { } message) Handshake(packet.Level, message);
                    break;
                case >= 8 and <= 15:
                    var id = input.Int(); var position = (type & 4) != 0 ? input.Int() : 0;
                    var bytes = (type & 2) != 0 ? input.Vector() : input.Take(input.Left);
                    streams!.Receive(id, position, bytes, (type & 1) != 0); break;
                case 4:
                    var reset = input.Int(); input.Int(); var final = input.Int();
                    streams!.Receive(reset, final, [], true); streams.Reset.Add(reset); break;
                case 0x10: streams!.DataLimit = Math.Max(streams.DataLimit, input.Int()); break;
                case 0x11: var streamId = input.Int(); streams!.Grant(streamId, input.Int()); break;
                case 0x13: streams!.UniStreams = Math.Max(streams.UniStreams, input.Int()); break;
                case 5: case 0x15: input.Int(); input.Int(); break;
                case 0x12: case 0x14: case 0x16: case 0x17: case 0x19: input.Int(); break;
                case 0x18:
                    input.Int(); var retire = input.Int(); var cid = input.Take(input.Byte()); input.Take(16);
                    if (cid.Length is 0 or > 20) throw new InvalidDataException("Invalid new connection ID.");
                    if (retire > 0) peerId = cid;
                    break;
                case 0x1a: spaces[2]!.Pending.Enqueue([0x1b, .. input.Take(8)]); break;
                case 0x1b: input.Take(8); break;
                case 0x1c: case 0x1d:
                    input.Int(); if (type == 0x1c) input.Int(); input.Vector(); Stop(); return;
                default: throw new InvalidDataException("Unknown QUIC frame.");
            }
        }
    }
    public List<byte[]> Flush()
    {
        if (tls.Complete && !Closed)
            foreach (var frame in streams!.Frames()) spaces[2]!.Pending.Enqueue(frame);
        var output = new List<byte[]>();
        for (var level = 0; level < spaces.Length; level++)
        {
            if (spaces[level] is not { } space) continue;
            while (space.Pending.Count > 0 || space.WantAck)
            {
                var ack = space.WantAck ? QuicAcks.Encode(space.Received) : [];
                var include = space.Pending.Count > 0 && ack.Length + space.Pending.Peek().Length <= 1050;
                byte[] plain = [.. ack, .. (include ? space.Pending.Peek() : [])];
                if (plain.Length == 0) throw new InvalidDataException("Oversized outgoing QUIC frame.");
                var packet = level == 2 ? QuicPacket.Short(peerId, space.Next, plain, space.Write) : QuicPacket.Long(level, peerId, Id, space.Next, plain, space.Write);
                if (!validated && sent + packet.Length > received * 3) break;
                space.Next++; sent += packet.Length;
                if (include) space.Pending.Dequeue();
                space.WantAck = false; output.Add(packet);
            }
        }
        return output;
    }
    public void Stop()
    {
        Closed = true;
        foreach (var space in spaces) if (space is not null) { space.Pending.Clear(); space.WantAck = false; }
    }
    public void Abort()
    {
        Stop(); var level = tls.Complete ? 2 : spaces[1] is not null ? 1 : 0;
        spaces[level]!.Pending.Enqueue([0x1c, 7, 0, 0]);
    }
    public void Dispose() { tls.Dispose(); foreach (var space in spaces) space?.Dispose(); }
}
