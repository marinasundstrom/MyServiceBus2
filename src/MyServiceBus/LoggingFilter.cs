using MyServiceBus.Transport;

namespace MyServiceBus;

public class LoggingFilter<T> : IConsumeFilter<T>
{
    public async Task Send(ConsumeContext<T> context, ReceiveEndpointHandler<T> next)
    {
        Console.WriteLine($"[Log] Handling {typeof(T).Name}");
        await next(context);
    }
}
