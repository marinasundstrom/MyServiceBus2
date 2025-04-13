namespace MyServiceBus.Topology;

public interface ISendTopology<T>
{
    string GetCorrelationId(T message);
    void UseCorrelationId(Func<T, string> selector);
}
