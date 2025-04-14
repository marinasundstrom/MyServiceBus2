using MyServiceBus.Transport;

namespace MyServiceBus;

public class ConsumeContextImpl<T> : ConsumeContext<T>
{
    private readonly ReceiveContext<T> _receiveContext;

    public ConsumeContextImpl(ReceiveContext<T> receiveContext)
    {
        _receiveContext = receiveContext;
    }

    public T Message => _receiveContext.TryGetMessage(out var message) ? message : default!;

    public CancellationToken CancellationToken => _receiveContext.CancellationToken;
}