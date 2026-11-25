namespace Http3Streams.Application;

public sealed class Observatory : IRequestHandler
{
    public Task<(int Status, string Body)> HandleAsync(string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = path switch
        {
            "/" => (200, "Нічна обсерваторія · /moon · /saturn"),
            "/moon" => (200, "Місяць · замалювати кратери вздовж термінатора"),
            "/saturn" => (200, "Сатурн · роздивитися кільця в телескоп"),
            _ => (404, "Об’єкта немає у програмі спостережень")
        };
        return Task.FromResult(response);
    }
}
