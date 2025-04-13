namespace MyServiceBus.Topology;

public static class MessageTopology
{
    private static readonly Dictionary<Type, object> _topologies = new();

    public static IMessageTopology<TMessage> For<TMessage>()
    {
        if (!_topologies.TryGetValue(typeof(TMessage), out var topology))
        {
            topology = new MessageTopologyImpl<TMessage>();
            _topologies[typeof(TMessage)] = topology;
        }

        return (IMessageTopology<TMessage>)topology;
    }

    public static IMessageTopology For(Type type)
    {
        if (!_topologies.TryGetValue(type, out var topology))
        {
            var implType = typeof(MessageTopologyImpl<>).MakeGenericType(type);
            topology = Activator.CreateInstance(implType);
            _topologies[type] = topology;
        }

        return (IMessageTopology)topology;
    }
}
