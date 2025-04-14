using System.Text.Json;

using MyServiceBus.Topology;
using MyServiceBus.Transport;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MyServiceBus.RabbitMq;

public class RabbitMqTransport : ISendTransport, IPublishTransport, IReceiveTransport
{
    private readonly IBusTopology _topology;
    private readonly IChannel _channel;

    public RabbitMqTransport(IChannel channel, IBusTopology topology)
    {
        _channel = channel;
        _topology = topology;
    }

    public async Task Send<T>(T message, SendContext context)
    {
        var queue = _topology.For<T>().EntityName;
        await _channel.QueueDeclareAsync(queue, true, false, false);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        var props = new BasicProperties();
        props.CorrelationId = context.CorrelationId;

        await _channel.BasicPublishAsync("", queue, false, props, body);
    }

    public async Task Publish<T>(T message, PublishContext context)
    {
        await _channel.ExchangeDeclareAsync(context.ExchangeName, context.ExchangeType, durable: true);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);
        var props = new BasicProperties();

        await _channel.BasicPublishAsync(context.ExchangeName, "", false, props, body);
    }

    public async Task Subscribe<T>(string queue, ReceiveHandler<T> handler)
    {
        var exchange = _topology.For<T>().EntityName;
        await _channel.ExchangeDeclareAsync(exchange, "fanout", durable: true);
        await _channel.QueueDeclareAsync(queue, true, false, false);
        await _channel.QueueBindAsync(queue, exchange, "");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (s, ea) =>
        {
            await handler(new ReceiveContext<T>(ea.Body, ea.BasicProperties.Headers!, ea.CancellationToken));
            await _channel.BasicAckAsync(ea.DeliveryTag, false);
        };

        await _channel.BasicConsumeAsync(queue, autoAck: false, consumer);
    }
}