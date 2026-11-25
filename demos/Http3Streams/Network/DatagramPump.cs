using System.Net.Sockets;

namespace Http3Streams.Network;

internal static class DatagramPump
{
    // Keep one receive pending while the clock services handshake and recovery deadlines.
    public static async Task RunAsync(UdpClient socket, Action<UdpReceiveResult> receive, Action tick, CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(20));
        using var pending = CancellationTokenSource.CreateLinkedTokenSource(token);
        Task<UdpReceiveResult> datagram = socket.ReceiveAsync(pending.Token).AsTask();
        Task<bool> alarm = timer.WaitForNextTickAsync(pending.Token).AsTask();
        try
        {
            while (!token.IsCancellationRequested)
            {
                await Task.WhenAny(datagram, alarm).ConfigureAwait(false);
                if (datagram.IsCompleted)
                {
                    receive(await datagram.ConfigureAwait(false));
                    datagram = socket.ReceiveAsync(pending.Token).AsTask();
                }
                if (alarm.IsCompleted)
                {
                    await alarm.ConfigureAwait(false);
                    tick();
                    alarm = timer.WaitForNextTickAsync(pending.Token).AsTask();
                }
            }
        }
        finally
        {
            await pending.CancelAsync().ConfigureAwait(false);
            try
            {
                await Task.WhenAll(datagram, alarm).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (pending.IsCancellationRequested)
            {
            }
        }
    }
}
