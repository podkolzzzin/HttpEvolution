using Http3Streams.Quic;

namespace Http3Streams.Tests;

public sealed class PacketProtectionTests
{
    [Fact]
    public void OpensAndReproducesTheServerInitialFromRfc9001AppendixA3()
    {
        byte[] destination = Convert.FromHexString("8394c8f03e515708");
        byte[] header = Convert.FromHexString("c1000000010008f067a5502a4262b50040750001");
        byte[] payload = Convert.FromHexString(
            "02000000000600405a020000560303eefce7f7b37ba1d1632e96677825ddf739" +
            "88cfc79825df566dc5430b9a045a1200130100002e00330024001d00209d3c94" +
            "0d89690b84d08a60993c144eca684d1081287c834d5311bcf32bb9da1a002b00" +
            "020304");
        byte[] expected = Convert.FromHexString(
            "cf000000010008f067a5502a4262b5004075c0d95a482cd0991cd25b0aac406a" +
            "5816b6394100f37a1c69797554780bb38cc5a99f5ede4cf73c3ec2493a1839b3" +
            "dbcba3f6ea46c5b7684df3548e7ddeb9c3bf9c73cc3f3bded74b562bfb19fb84" +
            "022f8ef4cdd93795d77d06edbb7aaf2f58891850abbdca3d20398c276456cbc4" +
            "2158407dd074ee");

        using var keys = new QuicKeys(QuicKeys.Initial(destination, server: true));
        var packet = QuicPacket.Read(expected);
        long largest = -1;
        var decoded = keys.Open(packet.Bytes, packet.NumberOffset, ref largest);
        Assert.Equal(1UL, decoded.Number);
        Assert.Equal(payload, decoded.Data);
        Assert.Equal(expected, keys.Seal(header, packet.NumberOffset, 1, payload));
    }
}
