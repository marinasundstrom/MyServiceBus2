using MyServiceBus.Topology;
using MyServiceBus.Transport;

using RabbitMQ.Client;

namespace MyServiceBus.RabbitMq;

public class RabbitMqTransportFactory : ITransportFactory
{
    private readonly ConnectionFactory _factory = new ConnectionFactory { HostName = "localhost" };
    private readonly IBusTopology _topology;
    private IConnection connection;

    public RabbitMqTransportFactory(IBusTopology topology)
    {
        _topology = topology;
    }

    public async Task<IPublishTransport> CreatePublishTransport(CancellationToken cancellationToken = default)
    {
        connection ??= await _factory.CreateConnectionAsync(cancellationToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        return new RabbitMqTransport(channel, _topology);
    }

    public async Task<IReceiveTransport> CreateReceiveTransport(CancellationToken cancellationToken = default)
    {
        connection ??= await _factory.CreateConnectionAsync(cancellationToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        return new RabbitMqTransport(channel, _topology);
    }

    public async Task<ISendTransport> CreateSendTransport(CancellationToken cancellationToken = default)
    {
        connection ??= await _factory.CreateConnectionAsync(cancellationToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        return new RabbitMqTransport(channel, _topology);
    }
}
