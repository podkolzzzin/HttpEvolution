namespace Http3Streams.Application;

public interface IRequestHandler
{
    public Task<(int Status, string Body)> HandleAsync(string path, CancellationToken cancellationToken);
}
