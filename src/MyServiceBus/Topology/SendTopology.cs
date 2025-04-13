namespace MyServiceBus.Topology;

public static class SendTopology
{
    private static readonly Dictionary<Type, object> _topologies = new();

    public static ISendTopology<T> Send<T>()
    {
        if (!_topologies.TryGetValue(typeof(T), out var topology))
        {
            topology = new SendTopologyImpl<T>();
            _topologies[typeof(T)] = topology;
        }

        return (ISendTopology<T>)topology;
    }
}
