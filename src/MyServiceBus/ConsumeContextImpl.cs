using MyServiceBus.Transports;

namespace MyServiceBus;

public class ConsumeContextImpl<T> : ConsumeContext<T>
    where T : class
{
    private readonly ReceiveContext<T> _receiveContext;

    public ConsumeContextImpl(ReceiveContext<T> receiveContext)
    {
        _receiveContext = receiveContext;
    }

    public T Message => _receiveContext.TryGetMessage(out var message) ? message : default!;

    public CancellationToken CancellationToken => _receiveContext.CancellationToken;
}