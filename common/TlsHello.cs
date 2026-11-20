using System.Security.Cryptography;
using System.Text;

sealed class TlsHello
{
    public byte[] Random { get; }
    public byte[] Session { get; }
    public byte[] Share { get; } = [];
    public Dictionary<int, byte[]> Extensions { get; } = [];
    public List<string> Protocols { get; } = [];
    public TlsHello(byte[] message)
    {
        var outer = new Wire(message);
        if (outer.Int(1) != 1) throw new InvalidDataException("Expected ClientHello.");
        var input = new Wire(outer.Vector(3)); outer.End();
        if (input.Int() != 0x0303) throw new InvalidDataException("Invalid legacy version.");
        Random = input.Take(32);
        Session = input.Vector(1);
        if (Session.Length > 32 || !Pairs(input.Vector()).Contains(0x1301)) throw new InvalidDataException("TLS_AES_128_GCM_SHA256 required.");
        if (!input.Vector(1).AsSpan().SequenceEqual(new byte[] { 0 })) throw new InvalidDataException("Compression must be null.");
        var extensions = new Wire(input.Vector()); input.End();
        while (extensions.Left > 0)
            if (!Extensions.TryAdd(extensions.Int(), extensions.Vector())) throw new InvalidDataException("Duplicate TLS extension.");
        byte[] Required(int id) => Extensions.GetValueOrDefault(id) ?? throw new InvalidDataException("Missing TLS extension.");
        if (!Pairs(Vector(Required(43), 1)).Contains(0x0304)) throw new InvalidDataException("TLS 1.3 required.");
        if (!Pairs(Vector(Required(13))).Contains(0x0403)) throw new InvalidDataException("ECDSA P-256 signature required.");
        if (!Pairs(Vector(Required(10))).Contains(23)) throw new InvalidDataException("P-256 key exchange required.");
        if (Extensions.TryGetValue(51, out var shares))
        {
            var keys = new Wire(Vector(shares));
            while (keys.Left > 0)
            {
                var group = keys.Int(); var key = keys.Vector();
                if (group == 23) Share = key;
            }
        }
        if (Extensions.TryGetValue(16, out var protocols))
        {
            var names = new Wire(Vector(protocols));
            while (names.Left > 0) Protocols.Add(Encoding.ASCII.GetString(names.Vector(1)));
        }
    }
    static byte[] Vector(byte[] data, int width = 2)
    {
        var input = new Wire(data); var value = input.Vector(width); input.End(); return value;
    }
    static IEnumerable<int> Pairs(byte[] data)
    {
        var input = new Wire(data);
        while (input.Left > 0) yield return input.Int();
    }
    public byte[] Reply(byte[]? share)
    {
        var random = share is null
            ? Convert.FromHexString("cf21ad74e59a6111be1d8c021e65b891c2a211167abb8c5e079e09e2c8a8339c")
            : RandomNumberGenerator.GetBytes(32);
        byte[] key = share is null ? Wire.Size(23) : [.. Wire.Size(23), .. Wire.Vector(share)];
        byte[] extensions = [.. Wire.Extension(43, [3, 4]), .. Wire.Extension(51, key)];
        return Wire.Message(2, [3, 3, .. random, .. Wire.Vector(Session, 1), 0x13, 1, 0, .. Wire.Vector(extensions)]);
    }
}
