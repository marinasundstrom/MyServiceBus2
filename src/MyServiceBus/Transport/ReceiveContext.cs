namespace MyServiceBus.Transport;

public class ReceiveContext<T>
{
    public ReceiveContext(T message, IDictionary<string, object?> transportHeaders, CancellationToken cancellationToken)
    {
        Message = message;
        TransportHeaders = transportHeaders;
        CancellationToken = cancellationToken;
    }

    public T Message { get; set; }
    public IDictionary<string, object?> TransportHeaders { get; }
    public CancellationToken CancellationToken { get; }
}
