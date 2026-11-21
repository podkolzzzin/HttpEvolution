using System.Text;

internal sealed class Http09Connection(Stream stream)
{
    public void Run()
    {
        using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
        var request = reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (request is not ["GET", var path, ..])
        {
            return;
        }

        var html = path == "/" ? "<h1>Hello, HTTP/0.9!</h1>\n" : "<h1>Not found</h1>\n";
        stream.Write(Encoding.ASCII.GetBytes(html));
    }
}
