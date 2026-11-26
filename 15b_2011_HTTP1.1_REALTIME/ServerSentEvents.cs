using System.Text;

internal static class ServerSentEvents
{
    public const string Page = """
        <!doctype html><html lang="en"><meta charset="utf-8"><title>Server-Sent Events</title>
        <h1>Server-Sent Events</h1><pre id="log"></pre>
        <script>
        const events = new EventSource("/events");
        events.onmessage = e => {
          document.querySelector("#log").textContent += e.lastEventId + " → " + e.data + "\n";
          if (e.lastEventId === "3") events.close();
        };
        </script></html>
        """;

    public static void Run(Stream stream, string? lastEventId)
    {
        const int finalId = 3;
        var previous = 0;
        if (lastEventId is not null && (!int.TryParse(lastEventId, out previous) || previous < 0 || previous > finalId))
        {
            stream.Write("HTTP/1.1 400 Bad Request\r\nConnection: close\r\nContent-Length: 0\r\n\r\n"u8);
            return;
        }

        if (previous == finalId)
        {
            stream.Write("HTTP/1.1 204 No Content\r\nConnection: close\r\n\r\n"u8);
            return;
        }

        stream.Write("HTTP/1.1 200 OK\r\nContent-Type: text/event-stream\r\nCache-Control: no-cache\r\nTransfer-Encoding: chunked\r\nConnection: close\r\n\r\n"u8);
        for (var id = previous + 1; id <= finalId; id++)
        {
            var payload = Encoding.UTF8.GetBytes($"id: {id}\ndata: tick {id}\n\n");
            stream.Write(Encoding.ASCII.GetBytes($"{payload.Length:X}\r\n"));
            stream.Write(payload);
            stream.Write("\r\n"u8);
            stream.Flush();
            if (id < finalId)
            {
                Thread.Sleep(1000);
            }
        }

        stream.Write("0\r\n\r\n"u8);
    }
}
