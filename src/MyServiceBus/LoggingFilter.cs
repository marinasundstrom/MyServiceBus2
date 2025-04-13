using MyServiceBus.Transport;

namespace MyServiceBus;

public class LoggingFilter<T> : IMessageFilter<T>
{
    public async Task Send(ReceiveContext<T> context, MessageHandlerDelegate<T> next)
    {
        Console.WriteLine($"[Log] Handling {typeof(T).Name}");
        await next(context);
    }
}
