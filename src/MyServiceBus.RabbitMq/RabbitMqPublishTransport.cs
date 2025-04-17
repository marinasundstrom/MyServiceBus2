using System.Text.Json;

using MyServiceBus.Topology;
using MyServiceBus.Transports;

using RabbitMQ.Client;

namespace MyServiceBus.RabbitMq;

public class RabbitMqPublishTransport : IPublishTransport
{
    private readonly IBusTopology _topology;
    private readonly IChannel _channel;

    public RabbitMqPublishTransport(IChannel channel, IBusTopology topology)
    {
        _channel = channel;
        _topology = topology;
    }

    public async Task Publish<T>(PublishContext<T> context)
    {
        await _channel.ExchangeDeclareAsync(context.ExchangeName, context.ExchangeType, durable: true);

        var body = JsonSerializer.SerializeToUtf8Bytes(context.Message);
        var props = new BasicProperties();

        await _channel.BasicPublishAsync(context.ExchangeName, "", false, props, body);
    }
}
