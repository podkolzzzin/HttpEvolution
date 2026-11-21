using System.Security.Cryptography;
using System.Text;

// A disposable P-256 share and valid TLS1.3 ClientHello for the in-memory demos.
internal static class DemoClientHello
{
    public static byte[] Create(
        string protocol = "http/1.1",
        byte[]? transportParameters = null,
        bool includeKeyShare = true
    )
    {
        using var key = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var point = key.ExportParameters(false).Q;
        byte[] share = [4, .. point.X!, .. point.Y!];
        byte[] extensions = [
            ..Wire.Extension(43, [2, 3, 4]),
            ..Wire.Extension(13, [0, 2, 4, 3]),
            ..Wire.Extension(10, [0, 2, 0, 23]),
            ..(includeKeyShare
                ? Wire.Extension(51, Wire.Vector([0, 23, ..Wire.Vector(share)]))
                : []),
            ..Wire.Extension(
                16,
                Wire.Vector(Wire.Vector(Encoding.ASCII.GetBytes(protocol), 1))
            )
        ];
        if (transportParameters is not null)
        {
            extensions = [.. extensions, .. Wire.Extension(57, transportParameters)];
        }

        return Wire.Message(
            1,
            [
                3,
                3,
                ..RandomNumberGenerator.GetBytes(32),
                0,
                0,
                2,
                0x13,
                1,
                1,
                0,
                ..Wire.Vector(extensions)
            ]
        );
    }
}
