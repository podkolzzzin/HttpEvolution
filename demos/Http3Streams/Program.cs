using System.Net;
using System.Security.Cryptography;
using Http3Streams.Application;
using Http3Streams.Transport;

namespace Http3Streams;

internal static class Program
{
    private static readonly IPEndPoint Endpoint = new(IPAddress.Loopback, 8456);

    public static async Task<int> Main(string[] args)
    {
        if (args is not ([] or ["server"] or ["client", _]))
        {
            Console.Error.WriteLine("Usage: Http3Streams [server | client <certificate-sha256>]");
            return 2;
        }

        using var shutdown = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            shutdown.Cancel();
        };

        try
        {
            if (args is ["client", var fingerprint])
            {
                await ShowObservationsAsync(Endpoint, fingerprint, shutdown.Token);
            }
            else
            {
                await HostAsync(runClient: args.Length == 0, shutdown.Token);
            }

            return 0;
        }
        catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
        {
            return 0;
        }
        catch (Exception error) when (error is IOException or InvalidDataException or CryptographicException or System.Security.Authentication.AuthenticationException or System.Net.Sockets.SocketException or TimeoutException)
        {
            Console.Error.WriteLine(error.Message);
            return 1;
        }
    }

    private static async Task HostAsync(bool runClient, CancellationToken cancellationToken)
    {
        using var certificate = DemoCertificate.Create();
        var fingerprint = certificate.GetCertHashString(HashAlgorithmName.SHA256);
        var endpoint = runClient ? new IPEndPoint(IPAddress.Loopback, 0) : Endpoint;
        await using var server = await Http3Server.ListenAsync(endpoint, certificate, new Observatory(), cancellationToken);
        Console.WriteLine($"Нічна обсерваторія · https://{server.LocalEndPoint} · HTTP/3");

        if (!runClient)
        {
            Console.WriteLine($"dotnet run --project demos/Http3Streams -- client {fingerprint}");
            await server.RunAsync(cancellationToken);
            return;
        }

        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var serving = server.RunAsync(lifetime.Token);
        try
        {
            await ShowObservationsAsync(server.LocalEndPoint, fingerprint, cancellationToken);
        }
        finally
        {
            await lifetime.CancelAsync();
            try
            {
                await serving;
            }
            catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
            {
                // The listener was waiting for the next connection.
            }
        }
    }

    private static async Task ShowObservationsAsync(IPEndPoint endpoint, string fingerprint, CancellationToken token)
    {
        await using var client = await Http3Client.ConnectAsync(endpoint, fingerprint, token);
        string[] paths = ["/", "/moon", "/saturn"];
        var requests = paths.Select(async path =>
        {
            var response = await client.GetAsync(path, token);
            return $"stream {response.StreamId,2}   GET {path,-8}   HTTP/3 {response.Status}   {response.Body}";
        });
        foreach (var line in await Task.WhenAll(requests))
        {
            Console.WriteLine(line);
        }
    }
}
