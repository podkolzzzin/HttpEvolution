using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var mode = args[0];
var port = int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture);
using var certificate = X509CertificateLoader.LoadPkcs12FromFile(args[2], string.Empty);
var builder = WebApplication.CreateBuilder();
builder.Logging.ClearProviders();
builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, port, endpoint =>
{
    endpoint.Protocols = mode switch
    {
        "http11" or "http11-tls" => HttpProtocols.Http1,
        "h2" or "h2-tls" => HttpProtocols.Http2,
        "h3" => HttpProtocols.Http3,
        _ => throw new ArgumentException("Unknown protocol.", nameof(args)),
    };
    if (mode is "http11-tls" or "h2-tls" or "h3")
    {
        endpoint.UseHttps(https =>
        {
            https.ServerCertificate = certificate;
            https.SslProtocols = SslProtocols.Tls13;
        });
    }
}));
var app = builder.Build();
app.Lifetime.ApplicationStarted.Register(() => Console.WriteLine("ready"));
var body = "<h1>Hello, HTTP!</h1>\n"u8.ToArray();
app.Run(async context =>
{
    context.Response.ContentType = "text/html";
    context.Response.ContentLength = body.Length;
    await context.Response.Body.WriteAsync(body, context.RequestAborted);
});
await app.RunAsync();
