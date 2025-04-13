namespace MyServiceBus.Transport;

public class PublishContext
{
    public string ExchangeName { get; set; }
    public string ExchangeType { get; set; }
    public IDictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();
}
