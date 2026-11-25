using System.Net;
using System.Net.Quic;
using System.Security.Cryptography;
using Http3Streams.Application;
using Http3Streams.Transport;

namespace Http3Streams.Tests;

public sealed class LoopbackTests
{
    [QuicFact]
    public async Task SecondRequestCompletesWhileFirstStreamIsStillOpen()
    {
        var handler = new PausedObservation();
        await WithServerAsync(handler, async (endpoint, fingerprint, token) =>
        {
            await using var client = await Http3Client.ConnectAsync(endpoint, fingerprint, token);
            var first = client.GetAsync("/moon", token);
            await handler.Started.Task.WaitAsync(token);
            var second = await client.GetAsync("/absent", token);
            Assert.Equal(404, second.Status);
            Assert.False(first.IsCompleted);

            handler.Resume.SetResult();
            var moon = await first;
            Assert.Equal(200, moon.Status);
            Assert.Contains("Місяць", moon.Body);
            Assert.NotEqual(moon.StreamId, second.StreamId);
            Assert.Equal(0, moon.StreamId % 4);
            Assert.Equal(0, second.StreamId % 4);
        });
    }

    [QuicFact]
    public async Task StandardHttpClientReadsTheUtf8ResponseOverHttp3()
    {
        await WithServerAsync(new Observatory(), async (endpoint, fingerprint, token) =>
        {
            using var handler = new HttpClientHandler
            {
                UseProxy = false,
                ServerCertificateCustomValidationCallback = (_, cert, _, _) =>
                    cert?.GetCertHashString(HashAlgorithmName.SHA256) == fingerprint
            };
            using var client = new HttpClient(handler)
            {
                DefaultRequestVersion = HttpVersion.Version30,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
            };
            using var response = await client.GetAsync($"https://localhost:{endpoint.Port}/saturn", token);
            Assert.Equal(HttpVersion.Version30, response.Version);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Сатурн", await response.Content.ReadAsStringAsync(token));
        });
    }

    private static async Task WithServerAsync(IRequestHandler handler, Func<IPEndPoint, string, CancellationToken, Task> run)
    {
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var token = deadline.Token;
        using var certificate = DemoCertificate.Create();
        await using var server = await Http3Server.ListenAsync(new IPEndPoint(IPAddress.Loopback, 0), certificate, handler, token);
        var serving = server.RunAsync(token);
        try
        {
            await run(server.LocalEndPoint, certificate.GetCertHashString(HashAlgorithmName.SHA256), token);
        }
        finally
        {
            await deadline.CancelAsync();
            try
            {
                await serving;
            }
            catch (OperationCanceledException) when (deadline.IsCancellationRequested)
            {
            }
        }
    }

    private sealed class PausedObservation : IRequestHandler
    {
        private readonly Observatory observatory = new();
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Resume { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<(int Status, string Body)> HandleAsync(string path, CancellationToken token)
        {
            if (path == "/moon")
            {
                Started.SetResult();
                await Resume.Task.WaitAsync(token);
            }

            return await observatory.HandleAsync(path, token);
        }
    }

    private sealed class QuicFactAttribute : FactAttribute
    {
        public QuicFactAttribute()
        {
            if (!QuicListener.IsSupported || !QuicConnection.IsSupported)
            {
                Skip = "Loopback integration requires MsQuic.";
            }
        }
    }
}
