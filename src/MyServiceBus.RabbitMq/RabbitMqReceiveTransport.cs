using MyServiceBus.Topology;
using MyServiceBus.Transport;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MyServiceBus.RabbitMq;

public class RabbitMqReceiveTransport : IReceiveTransport
{
    private readonly IBusTopology _topology;
    private readonly IChannel _channel;

    public RabbitMqReceiveTransport(IChannel channel, IBusTopology topology)
    {
        _channel = channel;
        _topology = topology;
    }

    public async Task Subscribe<T>(string queue, ReceiveHandler<T> handler)
        where T : class
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