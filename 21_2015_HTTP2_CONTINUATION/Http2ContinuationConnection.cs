internal sealed class Http2ContinuationConnection(Stream stream) : Http2FlowControlConnection(stream)
{
    private const int MaxHeaderBlockSize = 65_536;

    protected override void HandleOtherFrame(Http2Frame frame)
    {
        if (frame.Type == Http2FrameType.Continuation)
        {
            throw new InvalidDataException("Unexpected CONTINUATION.");
        }

        base.HandleOtherFrame(frame);
    }

    protected override bool CanReadHeaders(Http2Frame frame) => true;

    protected override byte[] ReadHeaderBlock(Http2Frame first)
    {
        if (first.HasFlags(Http2Flags.EndHeaders))
        {
            return base.ReadHeaderBlock(first);
        }

        using var block = new MemoryStream();
        block.Write(HeaderFragment(first));
        Http2Frame next;
        do
        {
            next = ReadFrame();
            if (next.Type != Http2FrameType.Continuation || next.StreamId != first.StreamId)
            {
                throw new InvalidDataException("Interrupted header block.");
            }

            if (block.Length + next.Payload.Length > MaxHeaderBlockSize)
            {
                throw new InvalidDataException("Header block too large.");
            }

            block.Write(next.Payload);
        }
        while (!next.HasFlags(Http2Flags.EndHeaders));

        return block.ToArray();
    }
}
