using System.Globalization;
using System.Text;

namespace Http3Streams.Protocol;

public static class Http3Message
{
    public static Task WriteGetAsync(Stream stream, string authority, string path, CancellationToken cancellationToken)
    {
        return Http3Frame.WriteAsync(stream, Http3Frame.Headers, StaticQpack.EncodeGet(authority, path), cancellationToken);
    }

    public static async Task<string> ReadGetAsync(Stream stream, CancellationToken cancellationToken)
    {
        var headers = await ReadHeadersAsync(stream, cancellationToken);
        if (headers.Count != 4 || headers.GetValueOrDefault(":method") != "GET"
            || headers.GetValueOrDefault(":scheme") != "https" || string.IsNullOrEmpty(headers.GetValueOrDefault(":authority"))
            || !headers.TryGetValue(":path", out string? path) || !path.StartsWith('/'))
        {
            throw new InvalidDataException("Expected an HTTPS GET with an authority and path.");
        }

        while (await NextMessageFrameAsync(stream, cancellationToken) is { } frame)
        {
            if (frame is not { Type: Http3Frame.Data, Payload.Length: 0 })
            {
                throw new InvalidDataException("The demo accepts GET requests without a body or trailers.");
            }
        }

        return path;
    }

    public static async Task WriteResponseAsync(Stream stream, int status, string body, CancellationToken cancellationToken)
    {
        byte[] payload = Encoding.UTF8.GetBytes(body);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(payload.Length, Http3Frame.MaxPayload);
        await Http3Frame.WriteAsync(stream, Http3Frame.Headers, StaticQpack.EncodeResponse(status), cancellationToken);
        await Http3Frame.WriteAsync(stream, Http3Frame.Data, payload, cancellationToken);
    }

    public static async Task<(int Status, string Body)> ReadResponseAsync(Stream stream, CancellationToken cancellationToken)
    {
        var headers = await ReadHeadersAsync(stream, cancellationToken);
        if (headers.Count != 1 || !headers.TryGetValue(":status", out string? text) || text.Length != 3
            || !int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int status) || status is < 200 or > 599)
        {
            throw new InvalidDataException("Expected a final HTTP status.");
        }

        using var body = new MemoryStream();
        while (await NextMessageFrameAsync(stream, cancellationToken) is { } frame)
        {
            if (frame.Type != Http3Frame.Data || body.Length + frame.Payload.Length > Http3Frame.MaxPayload)
            {
                throw new InvalidDataException("Expected an HTTP body up to 64 KiB, without trailers.");
            }

            body.Write(frame.Payload);
        }

        return (status, Encoding.UTF8.GetString(body.GetBuffer(), 0, (int)body.Length));
    }

    private static async Task<Dictionary<string, string>> ReadHeadersAsync(Stream stream, CancellationToken cancellationToken)
    {
        var frame = await NextMessageFrameAsync(stream, cancellationToken);
        if (frame is not { Type: Http3Frame.Headers })
        {
            throw new InvalidDataException("A message must start with HEADERS.");
        }

        return StaticQpack.DecodePseudoHeaders(frame.Payload);
    }

    private static async Task<Http3Frame?> NextMessageFrameAsync(Stream stream, CancellationToken cancellationToken)
    {
        while (await Http3Frame.ReadAsync(stream, cancellationToken) is { } frame)
        {
            switch (frame.Type)
            {
                case Http3Frame.Data:
                case Http3Frame.Headers:
                    return frame;
                case 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 13:
                    throw new InvalidDataException($"Unexpected HTTP/3 frame {frame.Type} on the request stream.");
                    // HTTP/3 endpoints ignore unknown extension frame types.
            }
        }

        return null;
    }
}
