using MyServiceBus.Topology;
using MyServiceBus.Transport;

namespace MyServiceBus;

public class MessageBus : IMessageBus
{
    private readonly ITransport _transport;

    public IBusTopology Topology { get; }

    public MessageBus(IBusTopology topology, ITransport transport)
    {
        Topology = topology;
        _transport = transport;
    }

    public Task Send<T>(T message)
    {
        var context = new SendContext
        {
            CorrelationId = ((SendTopologyImpl<T>)Topology.Send<T>()).GetCorrelationId(message)
        };

        return _transport.Send(message, context);
    }

    public Task Publish<T>(T message)
    {
        var pubTopology = Topology.Publish<T>();
        if (pubTopology.Exclude)
            return Task.CompletedTask;

        var context = new PublishContext
        {
            ExchangeName = Topology.For<T>().EntityName,
            ExchangeType = pubTopology.ExchangeType
        };

        return _transport.Publish(message, context);
    }

    public Task ReceiveEndpoint<T>(string queue, ReceiveEndpointHandler<T> handler)
    {
        return _transport.Subscribe<T>(queue, (receiveContext) => handler(new ConsumeContextImpl<T>(receiveContext)));
    }
}
