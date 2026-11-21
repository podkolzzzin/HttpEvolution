using System.Text;

internal sealed class Http10HeadConnection(Stream stream)
{
    public void Run()
    {
        using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
        var request = reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (request is not [var method, var path, ..] || method is not ("GET" or "HEAD"))
        {
            return;
        }

        var versioned = request.Length > 2;
        if (!versioned && method == "HEAD")
        {
            return;
        }

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
            stream.Write(Encoding.ASCII.GetBytes($"HTTP/1.0 {status}\r\nContent-Type: text/html\r\nContent-Length: {body.Length}\r\n\r\n"));
        }

        if (method == "GET")
        {
            stream.Write(body);
        }
    }
}
