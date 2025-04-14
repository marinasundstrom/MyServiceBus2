namespace MyServiceBus.Transport;

public class PublishContext<T>
{
    public T Message { get; set; }
    public string ExchangeName { get; set; }
    public string ExchangeType { get; set; }
    public IDictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();
}
