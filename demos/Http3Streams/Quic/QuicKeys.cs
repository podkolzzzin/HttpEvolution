using System.Security.Cryptography;
using Http3Streams.Handshake;

namespace Http3Streams.Quic;

internal readonly record struct DecryptedQuicPacket(byte Flags, ulong Number, byte[] Data);

// QUIC v1 packet and header protection (RFC 9001 section 5).
internal sealed class QuicKeys : IDisposable
{
    private readonly AesGcm payload;
    private readonly byte[] iv;
    private readonly Aes header;
    public QuicKeys(byte[] secret)
    {
        payload = new AesGcm(TlsHkdf.ExpandLabel(secret, "quic key", [], 16), 16);
        iv = TlsHkdf.ExpandLabel(secret, "quic iv", [], 12);
        header = Aes.Create();
        header.Key = TlsHkdf.ExpandLabel(secret, "quic hp", [], 16);
    }

    public static byte[] Initial(byte[] destination, bool server)
    {
        var salt = Convert.FromHexString("38762cf7f55934b34d179ae6a4c80cadccbb7f0a");
        return TlsHkdf.ExpandLabel(HMACSHA256.HashData(salt, destination), server ? "server in" : "client in", []);
    }

    private byte[] Nonce(ulong number)
    {
        var nonce = iv.ToArray();
        for (var i = 0; i < 8; i++)
        {
            nonce[^(i + 1)] ^= (byte)(number >> (8 * i));
        }

        return nonce;
    }

    private byte[] Mask(byte[] packet, int numberOffset)
    {
        if (numberOffset < 1 || numberOffset + 20 > packet.Length)
        {
            throw new InvalidDataException("Truncated header protection sample.");
        }

        return header.EncryptEcb(packet.AsSpan(numberOffset + 4, 16), PaddingMode.None);
    }

    public byte[] Seal(byte[] additional, int numberOffset, ulong number, byte[] plain)
    {
        additional = additional.AsSpan().ToArray();
        var length = (additional[0] & 3) + 1;
        if (numberOffset < 1 || additional.Length != numberOffset + length || number >= (1UL << 62))
        {
            throw new InvalidDataException("Invalid packet header.");
        }

        for (var i = 0; i < length; i++)
        {
            additional[numberOffset + length - i - 1] = (byte)(number >> (8 * i));
        }

        var packet = new byte[additional.Length + plain.Length + 16];
        additional.CopyTo(packet, 0);
        payload.Encrypt(Nonce(number), plain, packet.AsSpan(additional.Length, plain.Length), packet.AsSpan(additional.Length + plain.Length), additional);
        var mask = Mask(packet, numberOffset);
        packet[0] ^= (byte)(mask[0] & ((packet[0] & 128) != 0 ? 15 : 31));
        for (var i = 0; i < length; i++)
        {
            packet[numberOffset + i] ^= mask[i + 1];
        }

        return packet;
    }

    public DecryptedQuicPacket Open(byte[] packet, int numberOffset, ref long largest)
    {
        var mask = Mask(packet, numberOffset);
        var flags = (byte)(packet[0] ^ (mask[0] & ((packet[0] & 128) != 0 ? 15 : 31)));
        var length = (flags & 3) + 1;
        var additional = packet[..(numberOffset + length)];
        additional[0] = flags;
        ulong truncated = 0;
        for (var i = 0; i < length; i++)
        {
            additional[numberOffset + i] ^= mask[i + 1];
            truncated = (truncated << 8) | additional[numberOffset + i];
        }

        var expected = (ulong)(largest + 1);
        var window = 1UL << (8 * length);
        var half = window / 2;
        var number = (expected & ~(window - 1)) | truncated;
        if (number + half <= expected && number + window < (1UL << 62))
        {
            number += window;
        }
        else if (number > expected + half && number >= window)
        {
            number -= window;
        }

        if (number >= (1UL << 62))
        {
            throw new InvalidDataException("Packet number overflow.");
        }

        var data = new byte[packet.Length - additional.Length - 16];
        payload.Decrypt(Nonce(number), packet.AsSpan(additional.Length, data.Length), packet.AsSpan(packet.Length - 16), data, additional);
        largest = Math.Max(largest, (long)number);
        return new(flags, number, data);
    }

    public void Dispose()
    {
        payload.Dispose();
        header.Dispose();
    }
}
