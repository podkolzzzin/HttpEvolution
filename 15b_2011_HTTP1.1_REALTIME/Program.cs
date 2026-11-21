using static DemoMode;

return DemoCommand.Run(args, Sse | WebSocket, Sse, options =>
{
    RealtimeServer.Run(options.Has(WebSocket));
    return 0;
});
