internal readonly record struct TlsRecord(byte Type, byte[] Data);

internal interface IRecordLayer : IDisposable
{
    public Stream Transport
    {
        get;
    }

    public TlsRecord Read();
    public void Write(byte type, byte[] data);
}

internal static class RecordMessages
{
    public static byte[] Read(IRecordLayer records, ref byte[] pending)
    {
        while (true)
        {
            if (pending.Length >= 4)
            {
                var length = (pending[1] << 16) | (pending[2] << 8) | pending[3];
                if (length > 65536)
                {
                    throw new IOException("Handshake message too large.");
                }

                if (pending.Length >= length + 4)
                {
                    var message = pending[..(length + 4)];
                    pending = pending[(length + 4)..];
                    return message;
                }
            }

            var (type, data) = records.Read();
            if (type != 22)
            {
                throw new IOException("Expected handshake.");
            }

            pending = [.. pending, .. data];
        }
    }
}
