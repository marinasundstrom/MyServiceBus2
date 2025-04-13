namespace MyServiceBus.Topology;

public class SendTopologyImpl<T> : ISendTopology<T>
{
    private Func<T, string> _correlationIdSelector = _ => null;

    public void UseCorrelationId(Func<T, string> selector)
    {
        _correlationIdSelector = selector;
    }

    public string GetCorrelationId(T message)
    {
        return _correlationIdSelector(message);
    }
}
