using System.Text.Json;

using MyServiceBus.Topology;
using MyServiceBus.Transports;

using RabbitMQ.Client;

namespace MyServiceBus.RabbitMq;

public class RabbitMqSendTransport : ISendTransport
{
    private readonly IBusTopology _topology;
    private readonly IChannel _channel;

    public RabbitMqSendTransport(IChannel channel, IBusTopology topology)
    {
        _channel = channel;
        _topology = topology;
    }

    public async Task Send<T>(SendContext<T> context)
    {
        var queue = _topology.For<T>().EntityName;
        await _channel.QueueDeclareAsync(queue, true, false, false);

        var body = context.Serialize();

        var props = new BasicProperties();
        props.CorrelationId = context.CorrelationId;

        await _channel.BasicPublishAsync("", queue, false, props, body);
    }
}
