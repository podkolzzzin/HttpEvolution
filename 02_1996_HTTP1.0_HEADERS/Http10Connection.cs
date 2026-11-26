using System.Text;

internal sealed class Http10Connection(Stream stream)
{
    public void Run()
    {
        using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
        var request = reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (request is not ["GET", var path, ..])
        {
            return;
        }

        var versioned = request.Length > 2;
        if (versioned)
        {
            while (reader.ReadLine() is { Length: > 0 })
            {
            }
        }

        var status = path == "/" ? "200 OK" : "404 Not Found";
        var html = path == "/" ? "<h1>Hello, HTTP!</h1>\n" : "<h1>Not found</h1>\n";
        var body = Encoding.ASCII.GetBytes(html);
        if (versioned)
        {
            var headers = $"""
                HTTP/1.0 {status}
                Content-Type: text/html
                Content-Length: {body.Length}
                """.ReplaceLineEndings("\r\n") + "\r\n\r\n";

            stream.Write(Encoding.ASCII.GetBytes(headers));
        }

        stream.Write(body);
    }
}
