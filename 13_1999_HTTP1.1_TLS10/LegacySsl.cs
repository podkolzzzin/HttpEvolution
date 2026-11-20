using System.Security.Cryptography;
using System.Buffers.Binary;

static partial class LegacySsl
{
    static byte[] HelloRandom()
    {
        var random = RandomNumberGenerator.GetBytes(32);
        BinaryPrimitives.WriteUInt32BigEndian(random, (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        return random;
    }

    static byte[] Expand(byte[] secret, string label, byte[] seed, int length, byte minor) =>
        minor == 0
            ? Ssl3Crypto.Expand(secret, seed, length)
            : Tls10Crypto.Prf(secret, label, seed, length);
    static byte[] Finished(byte[] master, byte[] transcript, bool client, byte minor) =>
        minor == 0
            ? Ssl3Crypto.Finished(master, transcript, client)
            : Tls10Crypto.Finished(master, transcript, client);
    sealed class Exchange(LegacyRecords records, byte minor)
    {
        public byte[] Transcript = [];
        public byte[] Read(byte expected)
        {
            var message = records.Handshake();
            if (message[0] != expected)
            {
                throw new IOException("Unexpected SSL handshake message.");
            }

            Transcript = [.. Transcript, .. message];
            return message[4..];
        }

        public void Send(byte type, byte[] body)
        {
            var message = Wire.Message(type, body);
            records.Write(22, message);
            Transcript = [.. Transcript, .. message];
        }

        public void Verify(byte[] master, bool client)
        {
            var expected = Finished(master, Transcript, client, minor);
            if (!CryptographicOperations.FixedTimeEquals(expected, Read(20)))
            {
                throw new IOException("SSL Finished verification failed.");
            }
        }
    }

    static void Keys(LegacyRecords records, byte[] master, byte[] client, byte[] server, bool serverSide, byte minor)
    {
        var block = Expand(master, "key expansion", [.. server, .. client], 104, minor);
        records.Keys(block, serverSide);
        CryptographicOperations.ZeroMemory(block);
    }

    public static TlsStream? TryAccept(Stream transport, byte minor = 0)
    {
        var records = new LegacyRecords(transport, minor);
        byte[] master = [];
        try
        {
            if (transport.CanTimeout)
            {
                transport.ReadTimeout = transport.WriteTimeout = 5000;
            }

            var exchange = new Exchange(records, minor);
            var hello = new Wire(exchange.Read(1));
            if (hello.Int() != (0x0300 | minor))
            {
                throw new IOException("Unexpected SSL/TLS version.");
            }

            var client = hello.Take(32);
            hello.Vector(1);
            if (!hello.Vector().Chunk(2).Any(pair => pair.AsSpan().SequenceEqual(new byte[] { 0, 10 }))
                || !hello.Vector(1).Contains((byte)0))
            {
                throw new IOException("RSA/3DES/SHA cipher required.");
            }

            if (minor != 0 && hello.Left > 0)
            {
                hello.Vector();
            }

            hello.End();
            var server = HelloRandom();
            exchange.Send(2, [3, minor, .. server, 0, 0, 10, 0]);
            exchange.Send(11, Wire.Vector(Wire.Vector(LegacyIdentity.Der, 3), 3));
            exchange.Send(14, []);
            var encrypted = exchange.Read(16);
            if (minor != 0)
            {
                var vector = new Wire(encrypted);
                encrypted = vector.Vector();
                vector.End();
            }

            var premaster = LegacyIdentity.Premaster(encrypted, minor);
            master = Expand(premaster, "master secret", [.. client, .. server], 48, minor);
            CryptographicOperations.ZeroMemory(premaster);
            Keys(records, master, client, server, true, minor);
            records.ReadChange();
            exchange.Verify(master, true);
            records.WriteChange();
            exchange.Send(20, Finished(master, exchange.Transcript, false, minor));
            return new TlsStream(records);
        }
        catch (Exception error) when (error is IOException or InvalidDataException or CryptographicException)
        {
            try
            {
                records.Write(21, [2, 40]);
            }
            catch (IOException)
            {
            }

            records.Dispose();
            return null;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(master);
        }
    }
}
