using MyServiceBus.Topology;

namespace MyServiceBus;

public interface IMessageBus
{
    IBusTopology Topology { get; }

    Task Publish<T>(T message)
        where T : class;
    Task ReceiveEndpoint<T>(string queue, ReceiveEndpointHandler<T> handler)
        where T : class;
    Task Send<T>(T message)
        where T : class;

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
