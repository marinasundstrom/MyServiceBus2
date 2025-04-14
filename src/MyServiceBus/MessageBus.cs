
using MyServiceBus.Topology;
using MyServiceBus.Transport;

namespace MyServiceBus;

public class MessageBus : IMessageBus
{
    private readonly ITransportFactory _transportFactory;

    public IBusTopology Topology { get; }

    public MessageBus(IBusTopology topology, ITransportFactory transportFactory)
    {
        Topology = topology;
        _transportFactory = transportFactory;
    }

    public async Task Send<T>(T message)
    {
        var context = new SendContext<T>
        {
            Message = message,
            CorrelationId = ((SendTopologyImpl<T>)Topology.Send<T>()).GetCorrelationId(message)
        };

        var transport = await _transportFactory.CreateSendTransport();

        await transport.Send(context);
    }

    public async Task Publish<T>(T message)
    {
        var pubTopology = Topology.Publish<T>();
        if (pubTopology.Exclude)
            return;

        var context = new PublishContext
        {
            ExchangeName = Topology.For<T>().EntityName,
            ExchangeType = pubTopology.ExchangeType
        };

        var transport = await _transportFactory.CreatePublishTransport();

        await transport.Publish(message, context);
    }

    public async Task ReceiveEndpoint<T>(string queue, ReceiveEndpointHandler<T> handler)
    {
        var transport = await _transportFactory.CreateReceiveTransport();

        await transport.Subscribe<T>(queue, (receiveContext) => handler(new ConsumeContextImpl<T>(receiveContext)));
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}