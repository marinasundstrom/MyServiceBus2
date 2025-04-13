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
using MyServiceBus.Topology;

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
