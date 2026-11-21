using System.Security.Cryptography;
using System.Buffers.Binary;

internal static partial class LegacySsl
{
    private static byte[] HelloRandom()
    {
        var random = RandomNumberGenerator.GetBytes(32);
        BinaryPrimitives.WriteUInt32BigEndian(random, (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        return random;
    }

    private sealed class Exchange(LegacyRecords records)
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
            var expected = Ssl3Crypto.Finished(master, Transcript, client);
            if (!CryptographicOperations.FixedTimeEquals(expected, Read(20)))
            {
                throw new IOException("SSL Finished verification failed.");
            }
        }
    }

    private static void Keys(LegacyRecords records, byte[] master, byte[] client, byte[] server, bool serverSide)
    {
        var block = Ssl3Crypto.Expand(master, [.. server, .. client], 104);
        records.Keys(block, serverSide);
        CryptographicOperations.ZeroMemory(block);
    }

    public static TlsStream? TryAccept(Stream transport)
    {
        var records = new LegacyRecords(transport);
        byte[] master = [];
        try
        {
            if (transport.CanTimeout)
            {
                transport.ReadTimeout = transport.WriteTimeout = 5000;
            }

            var exchange = new Exchange(records);
            var hello = new Wire(exchange.Read(1));
            if (hello.Int() != 0x0300)
            {
                throw new IOException("Expected SSL 3.0.");
            }

            var client = hello.Take(32);
            hello.Vector(1);
            if (!hello.UInt16Vector().AsSpan().Contains((ushort)10)
                || !hello.Vector(1).AsSpan().Contains((byte)0))
            {
                throw new IOException("RSA/3DES/SHA cipher required.");
            }

            hello.End();
            var server = HelloRandom();
            exchange.Send(2, [3, 0, .. server, 0, 0, 10, 0]);
            exchange.Send(11, Wire.Vector(Wire.Vector(LegacyIdentity.Der, 3), 3));
            exchange.Send(14, []);
            var premaster = LegacyIdentity.Premaster(exchange.Read(16));
            master = Ssl3Crypto.Expand(premaster, [.. client, .. server], 48);
            CryptographicOperations.ZeroMemory(premaster);
            Keys(records, master, client, server, true);
            records.ReadChange();
            exchange.Verify(master, true);
            records.WriteChange();
            exchange.Send(20, Ssl3Crypto.Finished(master, exchange.Transcript, false));
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
