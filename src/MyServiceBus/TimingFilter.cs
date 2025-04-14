using System.Diagnostics;

using MyServiceBus.Transport;

namespace MyServiceBus;

public class TimingFilter<T> : IConsumeFilter<T>
    where T : class
{
    public async Task Send(ConsumeContext<T> context, ReceiveEndpointHandler<T> next)
    {
        var stopwatch = Stopwatch.StartNew();

        await next(context); // call the next step in the chain (which may include other filters or the handler)

        stopwatch.Stop();
        Console.WriteLine($"[Timing] {typeof(T).Name} handled in {stopwatch.ElapsedMilliseconds}ms");
    }
}