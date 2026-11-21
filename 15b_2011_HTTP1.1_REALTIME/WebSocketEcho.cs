using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;

internal static class WebSocketEcho
{
    private const int MaxMessageBytes = 4096;

    public const string Page = """
        <!doctype html><html lang="uk"><meta charset="utf-8"><title>WebSocket echo</title>
        <h1>WebSocket echo</h1><pre id="log"></pre>
        <script>
        const socket = new WebSocket("ws://" + location.host + "/chat");
        const log = document.querySelector("#log");
        socket.onopen = () => {
          const text = "Привіт!";
          log.textContent = "Клієнт → " + text + "\n";
          socket.send(text);
        };
        socket.onmessage = e => {
          log.textContent += "Сервер → " + e.data + "\n";
          socket.close(1000);
        };
        </script></html>
        """;

    public static void Run(Stream stream, Dictionary<string, string> headers)
    {
        var key = headers.GetValueOrDefault("Sec-WebSocket-Key", "");
        Span<byte> nonce = stackalloc byte[16];
        if (!ContainsToken(headers.GetValueOrDefault("Connection", ""), "Upgrade") ||
            !ContainsToken(headers.GetValueOrDefault("Upgrade", ""), "websocket") ||
            key.Length != 24 || !Convert.TryFromBase64String(key, nonce, out var length) || length != nonce.Length)
        {
            stream.Write("HTTP/1.1 400 Bad Request\r\nConnection: close\r\nContent-Length: 0\r\n\r\n"u8);
            return;
        }

        if (headers.GetValueOrDefault("Sec-WebSocket-Version") != "13")
        {
            stream.Write("HTTP/1.1 400 Bad Request\r\nSec-WebSocket-Version: 13\r\nContent-Length: 0\r\nConnection: close\r\n\r\n"u8);
            return;
        }

        const string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        var accept = Convert.ToBase64String(SHA1.HashData(Encoding.ASCII.GetBytes(key + guid)));
        stream.Write(Encoding.ASCII.GetBytes(
            $"HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: {accept}\r\n\r\n"));
        EchoFrames(stream);
    }

    private static bool ContainsToken(string values, string token) =>
        values.Split(',').Any(value => value.Trim().Equals(token, StringComparison.OrdinalIgnoreCase));

    private static void EchoFrames(Stream stream)
    {
        using var message = new MemoryStream();
        var continuing = false;
        while (true)
        {
            var first = stream.ReadByte();
            if (first == -1)
            {
                return;
            }

            var second = stream.ReadByte();
            if (second == -1)
            {
                return;
            }

            var final = (first & 0x80) != 0;
            var opcode = first & 0x0F;
            var masked = (second & 0x80) != 0;
            var size = second & 0x7F;
            if (!masked || (first & 0x70) != 0 || (opcode >= 8 && (!final || size > 125)))
            {
                Close(stream, 1002);
                return;
            }

            if (size == 127)
            {
                Close(stream, 1009);
                return;
            }

            if (size == 126)
            {
                var extended = new byte[2];
                stream.ReadExactly(extended);
                size = BinaryPrimitives.ReadUInt16BigEndian(extended);
                if (size < 126)
                {
                    Close(stream, 1002);
                    return;
                }
            }

            if (opcode < 8 && message.Length + size > MaxMessageBytes)
            {
                Close(stream, 1009);
                return;
            }

            var mask = new byte[4];
            stream.ReadExactly(mask);
            var payload = new byte[size];
            stream.ReadExactly(payload);
            for (var i = 0; i < payload.Length; i++)
            {
                payload[i] ^= mask[i % 4];
            }

            switch (opcode)
            {
                case 0 when continuing:   // CONTINUATION
                case 1 when !continuing:  // TEXT
                    message.Write(payload);
                    continuing = !final;
                    if (final)
                    {
                        var text = message.GetBuffer().AsSpan(0, (int)message.Length);
                        if (!Utf8.IsValid(text))
                        {
                            Close(stream, 1007);
                            return;
                        }

                        WriteFrame(stream, 1, text);
                        message.SetLength(0);
                    }

                    break;
                case 2:
                    Close(stream, 1003); // This endpoint accepts text messages.
                    return;
                case 8:
                    ReplyToClose(stream, payload);
                    return;
                case 9:
                    WriteFrame(stream, 10, payload); // PING → PONG
                    break;
                case 10:
                    break;
                default:
                    Close(stream, 1002);
                    return;
            }
        }
    }

    private static void ReplyToClose(Stream stream, byte[] payload)
    {
        if (payload.Length == 1)
        {
            Close(stream, 1002);
            return;
        }

        if (payload.Length >= 2)
        {
            var code = BinaryPrimitives.ReadUInt16BigEndian(payload);
            if (code is < 1000 or 1004 or 1005 or 1006 or (> 1014 and < 3000) or >= 5000)
            {
                Close(stream, 1002);
                return;
            }

            if (!Utf8.IsValid(payload.AsSpan(2)))
            {
                Close(stream, 1007);
                return;
            }
        }

        WriteFrame(stream, 8, payload);
    }

    private static void Close(Stream stream, ushort code)
    {
        Span<byte> payload = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(payload, code);
        WriteFrame(stream, 8, payload);
    }

    private static void WriteFrame(Stream stream, byte opcode, ReadOnlySpan<byte> payload)
    {
        stream.WriteByte((byte)(0x80 | opcode)); // FIN, server-to-client frames are unmasked.
        if (payload.Length <= 125)
        {
            stream.WriteByte((byte)payload.Length);
        }
        else
        {
            stream.WriteByte(126);
            Span<byte> extended = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(extended, (ushort)payload.Length);
            stream.Write(extended);
        }

        stream.Write(payload);
    }
}
