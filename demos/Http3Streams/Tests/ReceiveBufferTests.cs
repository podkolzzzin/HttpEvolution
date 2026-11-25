using Http3Streams.Network;

namespace Http3Streams.Tests;

public sealed class ReceiveBufferTests
{
    [Fact]
    public void DeliversContiguousBytesOnceEvenWhenFinArrivesFirst()
    {
        var buffer = new ReceiveBuffer();
        buffer.Insert(3, "def"u8, fin: true);
        byte[] output = new byte[6];
        Assert.Equal(0, buffer.Read(output));
        Assert.False(buffer.AtEnd);

        buffer.Insert(0, "abcd"u8, fin: false);
        Assert.Equal(4, buffer.Read(output.AsSpan(0, 4)));
        buffer.Insert(3, "def"u8, fin: true);
        Assert.Equal(2, buffer.Read(output.AsSpan(4)));
        Assert.Equal("abcdef"u8.ToArray(), output);
        Assert.True(buffer.AtEnd);
    }

    [Fact]
    public void ASecondFinCannotChangeTheFinalOffset()
    {
        var buffer = new ReceiveBuffer();
        buffer.Insert(3, [], fin: true);
        Assert.Throws<InvalidDataException>(() => buffer.Insert(4, [], fin: true));
        Assert.Throws<InvalidDataException>(() => buffer.Insert(3, [42], fin: false));
    }
}
