using System.Text.Json;

namespace MyServiceBus.Transports;

public class ReceiveContext<T>
    where T : class
{
    private T? _message;
    private readonly ReadOnlyMemory<byte> _data;

    public ReceiveContext(ReadOnlyMemory<byte> data, IDictionary<string, object?> transportHeaders, CancellationToken cancellationToken)
    {
        _data = data;
        TransportHeaders = transportHeaders;
        CancellationToken = cancellationToken;
    }

    public bool TryGetMessage(out T message)
    {
        if (_message is null)
        {
            _message = JsonSerializer.Deserialize<T>(_data.ToArray());
            message = _message!;
            return true;
        }

        if (_message is T typedMessage)
        {
            message = typedMessage;
            return true;
        }

        message = default(T)!;
        return false;
    }
    public IDictionary<string, object?> TransportHeaders { get; }
    public CancellationToken CancellationToken { get; }
}
