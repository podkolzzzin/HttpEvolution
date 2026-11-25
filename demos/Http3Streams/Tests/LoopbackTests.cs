using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using Http3Streams.Application;
using Http3Streams.Transport;

namespace Http3Streams.Tests;

public sealed class LoopbackTests
{
    [Fact]
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

    [Fact]
    public async Task RefusesAnUnpinnedCertificateAndStillAcceptsTheNextClient()
    {
        await WithServerAsync(new Observatory(), async (endpoint, fingerprint, token) =>
        {
            await Assert.ThrowsAsync<AuthenticationException>(
                () => Http3Client.ConnectAsync(endpoint, new string('0', 64), token));

            await using var client = await Http3Client.ConnectAsync(endpoint, fingerprint, token);
            Assert.Equal(200, (await client.GetAsync("/saturn", token)).Status);
        });
    }

    [Fact]
    public async Task RetransmitsTheInitialWhenTheFirstDatagramIsLost()
    {
        await WithServerAsync(new Observatory(), async (server, fingerprint, token) =>
        {
            using var relay = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            var relayEndpoint = (IPEndPoint)relay.Client.LocalEndPoint!;
            using var forwarding = CancellationTokenSource.CreateLinkedTokenSource(token);

            async Task ForwardAsync()
            {
                // Discard the first datagram. Keep the same relay port in both directions.
                var first = await relay.ReceiveAsync(forwarding.Token);
                var client = first.RemoteEndPoint;
                while (true)
                {
                    var packet = await relay.ReceiveAsync(forwarding.Token);
                    var destination = packet.RemoteEndPoint.Equals(server) ? client : server;
                    await relay.SendAsync(packet.Buffer, destination, forwarding.Token);
                }
            }

            var pump = ForwardAsync();
            try
            {
                await using var client = await Http3Client.ConnectAsync(relayEndpoint, fingerprint, token);
                var result = await client.GetAsync("/moon", token);
                Assert.Equal(200, result.Status);
                Assert.Contains("Місяць", result.Body);
            }
            finally
            {
                await forwarding.CancelAsync();
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pump);
            }
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
}
