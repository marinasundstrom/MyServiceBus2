namespace MyServiceBus.Transport;

public class SendContext
{
    public string CorrelationId { get; set; }

    public IDictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();
}
