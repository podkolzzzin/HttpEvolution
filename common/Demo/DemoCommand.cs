using System.CommandLine;

[Flags]
internal enum DemoMode
{
    Http = 1,
    Ssl3 = 2,
    Tls10 = 4,
    Tls13 = 8,
    Http2 = 16,
    Http3 = 32,
    Ssl3Client = 64,
    Tls10Client = 128,
    Sse = 256,
    WebSocket = 512
}

internal sealed record DemoOptions(DemoMode Modes, string? Ssl3Pin, string? Tls10Pin)
{
    public bool Has(DemoMode mode) => (Modes & mode) != 0;
}

internal static class DemoCommand
{
    public static int Run(string[] args, DemoMode available, DemoMode defaultMode, Func<DemoOptions, int> run) =>
        RunAsync(args, available, defaultMode, options => Task.FromResult(run(options))).GetAwaiter().GetResult();

    public static Task<int> RunAsync(
        string[] args, DemoMode available, DemoMode defaultMode, Func<DemoOptions, Task<int>> run)
    {
        var command = new RootCommand("Run a protocol demo on loopback.");
        var modes = new Dictionary<DemoMode, Option<bool>>();
        Add(DemoMode.Http, "--http", "Serve cleartext HTTP/1.");
        Add(DemoMode.Ssl3, "--ssl3", "Serve HTTPS with SSL 3.0.");
        Add(DemoMode.Tls10, "--tls10", "Serve HTTPS with TLS 1.0.");
        Add(DemoMode.Tls13, "--tls", "Serve with TLS 1.3.");
        Add(DemoMode.Http2, "--h2", "Serve HTTP/2.");
        Add(DemoMode.Http3, "--h3", "Serve HTTP/3 over QUIC.");
        Add(DemoMode.Sse, "--sse", "Stream server-sent events over HTTP/1.1.");
        Add(DemoMode.WebSocket, "--ws", "Echo messages over a WebSocket connection.");
        var ssl3Client = AddClient(DemoMode.Ssl3Client, "--ssl3-client", "Run the pinned SSL 3.0 client.");
        var tls10Client = AddClient(DemoMode.Tls10Client, "--tls10-client", "Run the pinned TLS 1.0 client.");

        void Add(DemoMode mode, string name, string description)
        {
            if ((available & mode) != 0)
            {
                var option = new Option<bool>(name) { Description = description };
                command.Options.Add(option);
                modes.Add(mode, option);
            }
        }

        Option<string>? AddClient(DemoMode mode, string name, string description)
        {
            if ((available & mode) == 0)
            {
                return null;
            }

            var option = new Option<string>(name)
            {
                Description = description,
                HelpName = "PIN",
                Arity = ArgumentArity.ExactlyOne
            };
            command.Options.Add(option);
            return option;
        }

        command.SetAction((parse, _) =>
        {
            var selected = (DemoMode)0;
            var explicitMode = false;
            foreach (var (mode, option) in modes)
            {
                explicitMode |= parse.GetResult(option) is { Implicit: false };
                if (parse.GetValue(option))
                {
                    selected |= mode;
                }
            }

            return run(new DemoOptions(
                explicitMode ? selected : defaultMode,
                ssl3Client is null ? null : parse.GetValue(ssl3Client),
                tls10Client is null ? null : parse.GetValue(tls10Client)
            ));
        });

        return command.Parse(args).InvokeAsync();
    }

    public static int RunClient(Action client)
    {
        try
        {
            client();
            return 0;
        }
        catch (IOException error)
        {
            Console.Error.WriteLine(error.Message);
            return 1;
        }
    }
}
