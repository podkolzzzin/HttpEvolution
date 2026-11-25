using System.Security.Authentication;
using System.Security.Cryptography;
using Http3Streams.Handshake;

namespace Http3Streams.Tests;

public sealed class TlsHandshakeTests
{
    [Fact]
    public void BothPeersDeriveMatchingTrafficSecretsAndAuthenticateFinished()
    {
        using var certificate = DemoCertificate.Create();
        using var server = QuicTls.Server(certificate, [0x53]);
        using var client = QuicTls.Client(certificate.GetCertHashString(HashAlgorithmName.SHA256), [0x43]);
        var flights = server.Receive(0, client.StartClientHello());

        client.Receive(0, flights[0].Bytes);
        Assert.Equal(server.ClientHandshakeTraffic, client.ClientHandshakeTraffic);
        Assert.Equal(server.ServerHandshakeTraffic, client.ServerHandshakeTraffic);
        Assert.NotEqual(client.ClientHandshakeTraffic, client.ServerHandshakeTraffic);

        var messages = SplitFlight(flights[1].Bytes).ToArray();
        foreach (var message in messages[..^1])
        {
            client.Receive(1, message);
        }

        var finished = Assert.Single(client.Receive(1, messages[^1]));
        server.Receive(finished.Level, finished.Bytes);
        Assert.True(client.Complete);
        Assert.True(server.Complete);
        Assert.Equal(server.ClientTraffic, client.ClientTraffic);
        Assert.Equal(server.ServerTraffic, client.ServerTraffic);
        Assert.Equal(new byte[] { 0x43 }, server.PeerParameters);
        Assert.Equal(new byte[] { 0x53 }, client.PeerParameters);
    }

    [Theory]
    [InlineData(15)] // CertificateVerify
    [InlineData(20)] // Finished
    public void ClientRejectsModifiedAuthenticationMessage(int target)
    {
        using var certificate = DemoCertificate.Create();
        using var server = QuicTls.Server(certificate, []);
        using var client = QuicTls.Client(certificate.GetCertHashString(HashAlgorithmName.SHA256), []);
        var flights = server.Receive(0, client.StartClientHello());
        client.Receive(0, flights[0].Bytes);
        var messages = SplitFlight(flights[1].Bytes).ToArray();
        foreach (var message in messages.TakeWhile(message => message[0] != target))
        {
            client.Receive(1, message);
        }

        var tampered = messages.Single(message => message[0] == target);
        tampered[^1] ^= 1;
        if (target == 15)
        {
            Assert.Throws<AuthenticationException>(() => client.Receive(1, tampered));
        }
        else
        {
            Assert.Throws<CryptographicException>(() => client.Receive(1, tampered));
        }
        Assert.False(client.Complete);
        Assert.Empty(client.ClientTraffic);
    }

    private static IEnumerable<byte[]> SplitFlight(byte[] flight)
    {
        for (int offset = 0; offset < flight.Length;)
        {
            int length = (flight[offset + 1] << 16) | (flight[offset + 2] << 8) | flight[offset + 3];
            yield return flight[offset..(offset + 4 + length)];
            offset += 4 + length;
        }
    }
}
