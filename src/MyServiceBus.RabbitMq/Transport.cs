using System.Text.Json;
using MyServiceBus.Transport;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MyServiceBus.RabbitMq;

public class RabbitMqTransport : ITransport
{
    private readonly List<(IConnection conn, IChannel ch)> _activeConsumers = new();
    private readonly ConnectionFactory _factory = new ConnectionFactory { HostName = "localhost" };

    public async Task Send<T>(T message, SendContext context)
    {
        using var conn = await _factory.CreateConnectionAsync();
        using var ch = await conn.CreateChannelAsync();

        var queue = MessageTopology.For<T>().EntityName;
        await ch.QueueDeclareAsync(queue, true, false, false);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        var props = new BasicProperties();
        props.CorrelationId = context.CorrelationId;

        await ch.BasicPublishAsync("", queue, false, props, body);
    }

    public async Task Publish<T>(T message, PublishContext context)
    {
        using var conn = await _factory.CreateConnectionAsync();
        using var ch = await conn.CreateChannelAsync();

        await ch.ExchangeDeclareAsync(context.ExchangeName, context.ExchangeType, durable: true);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);
        var props = new BasicProperties();

        await ch.BasicPublishAsync(context.ExchangeName, "", false, props, body);
    }

    public async Task Subscribe<T>(string queue, MessageHandlerDelegate<T> handler)
    {
        var conn = await _factory.CreateConnectionAsync();
        var ch = await conn.CreateChannelAsync();

        _activeConsumers.Add((conn, ch));

        var exchange = MessageTopology.For<T>().EntityName;
        await ch.ExchangeDeclareAsync(exchange, "fanout", durable: true);
        await ch.QueueDeclareAsync(queue, true, false, false);
        await ch.QueueBindAsync(queue, exchange, "");

        var consumer = new AsyncEventingBasicConsumer(ch);
        consumer.ReceivedAsync += async (s, ea) =>
        {
            var message = JsonSerializer.Deserialize<T>(ea.Body.ToArray());
            await handler(new ReceiveContext<T>(message, ea.BasicProperties.Headers!, ea.CancellationToken));
            await ch.BasicAckAsync(ea.DeliveryTag, false);
        };

        await ch.BasicConsumeAsync(queue, autoAck: false, consumer);
    }
}