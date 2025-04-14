using MyServiceBus.Topology;

namespace MyServiceBus;

public interface IMessageBus
{
    IBusTopology Topology { get; }

    Task Publish<T>(T message);
    Task ReceiveEndpoint<T>(string queue, ReceiveEndpointHandler<T> handler);
    Task Send<T>(T message);
}
