namespace MyServiceBus.Topology;

public static class PublishTopology
{
    private static readonly Dictionary<Type, object> _topologies = new();

    public static IPublishTopology<TMessage> GetMessageTopology<TMessage>()
    {
        if (!_topologies.TryGetValue(typeof(TMessage), out var topology))
        {
            topology = new PublishTopologyImpl<TMessage>();
            _topologies[typeof(TMessage)] = topology;
        }

        return (IPublishTopology<TMessage>)topology;
    }

    public static IPublishTopology GetMessageTopology(Type messageType)
    {
        if (!_topologies.TryGetValue(messageType, out var topology))
        {
            var genericType = typeof(PublishTopologyImpl<>).MakeGenericType(messageType);
            topology = Activator.CreateInstance(genericType);
            _topologies[messageType] = topology;
        }

        return (IPublishTopology)topology;
    }
}
