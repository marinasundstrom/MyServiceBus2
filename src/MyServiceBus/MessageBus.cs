
using MyServiceBus.Topology;
using MyServiceBus.Transports;

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
        where T : class
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
        where T : class
    {
        var pubTopology = Topology.Publish<T>();
        if (pubTopology.Exclude)
            return;

        var context = new PublishContext<T>
        {
            Message = message,
            ExchangeName = Topology.For<T>().EntityName,
            ExchangeType = pubTopology.ExchangeType
        };

        var transport = await _transportFactory.CreatePublishTransport();

        await transport.Publish(context);
    }

    public async Task ReceiveEndpoint<T>(string queue, ReceiveEndpointHandler<T> handler)
        where T : class
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