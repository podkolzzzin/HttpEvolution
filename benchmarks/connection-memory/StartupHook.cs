internal static class StartupHook
{
    public static void Initialize()
    {
        var directory = Environment.GetEnvironmentVariable("HTTP_MEMORY_PROBE_DIRECTORY")
            ?? throw new InvalidOperationException("HTTP_MEMORY_PROBE_DIRECTORY is required.");
        var probe = new Thread(() => Sample(directory)) { IsBackground = true };
        probe.Start();
    }

    private static void Sample(string directory)
    {
        var request = Path.Combine(directory, "sample");
        var result = Path.Combine(directory, "result");
        var temporary = Path.Combine(directory, "result.tmp");

        while (true)
        {
            if (!File.Exists(request))
            {
                Thread.Sleep(5);
                continue;
            }

            File.Delete(request);
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            var bytes = GC.GetTotalMemory(forceFullCollection: false);
            File.WriteAllText(temporary, bytes.ToString(System.Globalization.CultureInfo.InvariantCulture));
            File.Move(temporary, result);
        }
    }
}
