// See https://aka.ms/new-console-template for more information

/*

var msgTopology = MessageTopology.For<MyMessage>();
msgTopology.SetEntityName("my-message");
Console.WriteLine($"Message EntityName: {msgTopology.EntityName}");

var sendTopology = SendTopology.Send<MyCommand>();
sendTopology.UseCorrelationId(x => x.CorrelationId);

var command = new MyCommand();
Console.WriteLine($"CorrelationId: {((SendTopologyImpl<MyCommand>)sendTopology).GetCorrelationId(command)}");

var pubTopology = PublishTopology.GetMessageTopology<MyMessage>();
pubTopology.Exclude = true;

Console.WriteLine($"MyMessage excluded from publish: {pubTopology.Exclude}");

var endpoint = new ReceiveEndpointConfigurator
{
    ConfigureConsumeTopology = false
};
endpoint.Bind<MyMessage>();

FakeMessageBus.Send(new MyCommand()); */

/*
cfg.ReceiveEndpoint("my-queue", e =>
{
    e.ConfigureConsumeTopology = false;
    e.Bind<MyMessage>(); // manual binding
});

static void DebugTopology<T>()
{
    var msg = MessageTopology.For<T>();
    var pub = PublishTopology.GetMessageTopology<T>();
    var send = (SendTopologyImpl<T>)SendTopology.Send<T>();

    Console.WriteLine($"[Topology] {typeof(T).Name}");
    Console.WriteLine($"  EntityName: {msg.EntityName}");
    Console.WriteLine($"  IsTemporary: {msg.IsTemporary}");
    Console.WriteLine($"  Exclude from publish: {pub.Exclude}");
    Console.WriteLine($"  ExchangeType: {pub.ExchangeType}");
}
*/

using MyServiceBus;

public class FakeMessageBus
{
    private readonly Dictionary<Type, object> _sendTopologies = new();
    private readonly Dictionary<Type, object> _publishTopologies = new();
    private readonly Dictionary<Type, object> _messageTopologies = new();


    public ISendTopology<T> Send<T>()
    {
        if (!_sendTopologies.TryGetValue(typeof(T), out var topology))
        {
            topology = new SendTopologyImpl<T>();
            _sendTopologies[typeof(T)] = topology;
        }

        return (ISendTopology<T>)topology;
    }

    public IPublishTopology<T> GetPublishTopology<T>()
    {
        if (!_publishTopologies.TryGetValue(typeof(T), out var topology))
        {
            topology = new PublishTopologyImpl<T>();
            _publishTopologies[typeof(T)] = topology;
        }

        return (IPublishTopology<T>)topology;
    }

    public IPublishTopology GetPublishTopology(Type type)
    {
        if (!_publishTopologies.TryGetValue(type, out var topology))
        {
            var genericType = typeof(PublishTopologyImpl<>).MakeGenericType(type);
            topology = Activator.CreateInstance(genericType);
            _publishTopologies[type] = topology;
        }

        return (IPublishTopology)topology;
    }

    public IMessageTopology<T> GetMessageTopology<T>()
    {
        if (!_messageTopologies.TryGetValue(typeof(T), out var topology))
        {
            topology = new MessageTopologyImpl<T>();
            _messageTopologies[typeof(T)] = topology;
        }

        return (IMessageTopology<T>)topology;
    }

    public void Send<T>(T message)
    {
        var topology = (SendTopologyImpl<T>)Send<T>();
        var correlationId = topology.GetCorrelationId(message);
        Console.WriteLine($"[Send] {typeof(T).Name} with CorrelationId = {correlationId}");
    }

    public void Publish<T>(T message)
    {
        var topology = (PublishTopologyImpl<T>)GetPublishTopology<T>();

        if (topology.Exclude)
        {
            Console.WriteLine($"[Publish] Skipped {typeof(T).Name} (excluded)");
            return;
        }

        var msgTopology = GetMessageTopology<T>();
        Console.WriteLine($"[Publish] Publishing {typeof(T).Name} to Exchange: '{msgTopology.EntityName}' ({topology.ExchangeType})");

        foreach (var iface in typeof(T).GetInterfaces())
        {
            var ifaceTopology = GetPublishTopology(iface);
            foreach (var boundType in ifaceTopology.BoundTypes)
            {
                Console.WriteLine($"[Publish] Also routed to bound type exchange: {boundType.Name}");
            }
        }
    }
}

public record MyMessage
{

}

public record MyCommand
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}

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

public class MessageTopologyImpl<TMessage> : IMessageTopology<TMessage>
{
    public string EntityName { get; private set; } = typeof(TMessage).Name;
    public bool IsTemporary { get; private set; } = false;

    public void SetEntityName(string entityName)
    {
        EntityName = entityName;
    }
}

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

public class PublishTopologyImpl<TMessage> : IPublishTopology<TMessage>
{
    public string ExchangeType { get; } = "fanout";

    public bool Exclude { get; set; } = false;

    private readonly List<Type> _boundTypes = new();

    public void AddMessagePublishTopology<TSub>() where TSub : TMessage
    {
        _boundTypes.Add(typeof(TSub));
    }

    public IEnumerable<Type> BoundTypes => _boundTypes;
}

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

public class ReceiveEndpointConfigurator : IConsumerTopology
{
    public bool ConfigureConsumeTopology { get; set; } = true;

    public void Bind<TMessage>()
    {
        Console.WriteLine($"Manually binding to {typeof(TMessage).Name}");
    }
}

public interface IMessageTopology
{
    string EntityName { get; }
    bool IsTemporary { get; }
}

public interface IMessageTopology<TMessage> : IMessageTopology
{
    void SetEntityName(string EntityName);
}

public interface IPublishTopology
{
    IEnumerable<Type> BoundTypes { get; }
}

public interface IPublishTopology<TMessage> : IPublishTopology
{
    string ExchangeType { get; }
    bool Exclude { get; set; }

    void AddMessagePublishTopology<TSub>() where TSub : TMessage;
}

public interface ISendTopology<T>
{
    string GetCorrelationId(T message);
    void UseCorrelationId(Func<T, string> selector);
}

public interface IConsumerTopology
{
    bool ConfigureConsumeTopology { get; }

    void Bind<TMessage>();
}