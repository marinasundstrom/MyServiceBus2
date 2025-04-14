namespace MyServiceBus.Topology;

public class DefaultBusTopology : IBusTopology
{
    private readonly Dictionary<Type, object> _messageTopologies = new();
    private readonly Dictionary<Type, object> _sendTopologies = new();
    private readonly Dictionary<Type, object> _publishTopologies = new();

    public IMessageTopology<TMessage> For<TMessage>()
    {
        if (!_messageTopologies.TryGetValue(typeof(TMessage), out var topology))
        {
            topology = new MessageTopologyImpl<TMessage>();
            _messageTopologies[typeof(TMessage)] = topology;
        }

        return (IMessageTopology<TMessage>)topology;
    }

    public IMessageTopology For(Type type)
    {
        if (!_messageTopologies.TryGetValue(type, out var topology))
        {
            var implType = typeof(MessageTopologyImpl<>).MakeGenericType(type);
            topology = Activator.CreateInstance(implType);
            _messageTopologies[type] = topology;
        }

        return (IMessageTopology)topology;
    }

    public ISendTopology<T> Send<T>()
    {
        if (!_sendTopologies.TryGetValue(typeof(T), out var topology))
        {
            topology = new SendTopologyImpl<T>();
            _sendTopologies[typeof(T)] = topology;
        }

        return (ISendTopology<T>)topology;
    }

    public IPublishTopology<TMessage> Publish<TMessage>()
    {
        if (!_publishTopologies.TryGetValue(typeof(TMessage), out var topology))
        {
            topology = new PublishTopologyImpl<TMessage>();
            _publishTopologies[typeof(TMessage)] = topology;
        }

        return (IPublishTopology<TMessage>)topology;
    }

    public IPublishTopology Publish(Type messageType)
    {
        if (!_publishTopologies.TryGetValue(messageType, out var topology))
        {
            var genericType = typeof(PublishTopologyImpl<>).MakeGenericType(messageType);
            topology = Activator.CreateInstance(genericType);
            _publishTopologies[messageType] = topology;
        }

        return (IPublishTopology)topology;
    }
}