
using System.Text.Json;

namespace MyServiceBus.Transport;

public class SendContext<T>
{
    public T Message { get; set; }

    public string CorrelationId { get; set; }

    public IDictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();

    public ReadOnlyMemory<byte> Serialize()
    {
        return JsonSerializer.SerializeToUtf8Bytes(Message);
    }
}
