using Http3Streams.Protocol;

namespace Http3Streams.Tests;

public sealed class MessageTests
{
    [Fact]
    public async Task ReadsAGetAcrossFragmentedReads()
    {
        using var stream = new FragmentedStream();
        await Http3Message.WriteGetAsync(stream, "localhost", "/moon", CancellationToken.None);
        stream.Position = 0;

        Assert.Equal("/moon", await Http3Message.ReadGetAsync(stream, CancellationToken.None));
    }

    [Theory]
    [InlineData(200, "Місяць")]
    [InlineData(404, "Об’єкта немає")]
    public async Task ResponseSurvivesFragmentedUtf8(int status, string body)
    {
        using var stream = new FragmentedStream();
        await Http3Message.WriteResponseAsync(stream, status, body, CancellationToken.None);
        stream.Position = 0;

        Assert.Equal((status, body), await Http3Message.ReadResponseAsync(stream, CancellationToken.None));
    }

    [Fact]
    public async Task ReadsARequestEncodedUsingOnlyRfcStaticIndexes()
    {
        // HEADERS with no dynamic references. GET, https, :authority "x", :path "/".
        byte[] frame = [0x01, 0x08, 0x00, 0x00, 0xd1, 0xd7, 0x50, 0x01, 0x78, 0xc1];
        using var stream = new MemoryStream(frame);
        Assert.Equal("/", await Http3Message.ReadGetAsync(stream, CancellationToken.None));
    }

    [Fact]
    public async Task SkipsAnExtensionBeforeResponseHeaders()
    {
        using var stream = new MemoryStream();
        stream.Write([0x21, 0x02, 0xda, 0x7a]);
        await Http3Message.WriteResponseAsync(stream, 200, "moon", CancellationToken.None);
        stream.Position = 0;
        Assert.Equal((200, "moon"), await Http3Message.ReadResponseAsync(stream, CancellationToken.None));
    }

    [Fact]
    public async Task RejectsAnIncompleteFrame()
    {
        using var stream = new MemoryStream([0x01, 0x08, 0x00, 0x00]);
        await Assert.ThrowsAsync<EndOfStreamException>(() => Http3Message.ReadGetAsync(stream, CancellationToken.None));
    }

    private sealed class FragmentedStream : MemoryStream
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default)
        {
            return base.ReadAsync(buffer[..Math.Min(1, buffer.Length)], token);
        }
    }
}
