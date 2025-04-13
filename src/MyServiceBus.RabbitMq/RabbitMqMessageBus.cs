using MyServiceBus.Topology;
using MyServiceBus.Transport;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System.Text.Json;

namespace MyServiceBus.RabbitMq;

public class RabbitMqMessageBus : IMessageBus
{
    private readonly ConnectionFactory _factory;

    public RabbitMqMessageBus(string host = "localhost")
    {
        _factory = new ConnectionFactory
        {
            HostName = host,
            //ConsumerDispatchConcurrency = 2
        };
    }

    public async Task Send<T>(T message)
    {
        var topology = (SendTopologyImpl<T>)SendTopology.Send<T>();
        var correlationId = topology.GetCorrelationId(message);

        using var conn = await _factory.CreateConnectionAsync();
        using var channel = await conn.CreateChannelAsync();

        var queueName = MessageTopology.For<T>().EntityName;

        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);

        var props = new BasicProperties();
        props.CorrelationId = correlationId;

        var body = JsonSerializer.SerializeToUtf8Bytes(message);
        await channel.BasicPublishAsync(exchange: "", routingKey: queueName, false, basicProperties: props, body: body);

        Console.WriteLine($"[RabbitMQ] Sent {typeof(T).Name} with CorrelationId: {correlationId}");
    }

    public async Task Publish<T>(T message)
    {
        var publishTopo = (PublishTopologyImpl<T>)PublishTopology.GetMessageTopology<T>();
        if (publishTopo.Exclude)
        {
            Console.WriteLine($"[RabbitMQ] Skipped publish of {typeof(T).Name} (excluded)");
            return;
        }

        var entityName = MessageTopology.For<T>().EntityName;
        var exchangeType = publishTopo.ExchangeType;

        using var conn = await _factory.CreateConnectionAsync();
        using var channel = await conn.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(entityName, exchangeType, durable: true);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        var props = new BasicProperties();
        props.MessageId = Guid.NewGuid().ToString();

        await channel.BasicPublishAsync(exchange: entityName, routingKey: "", false, basicProperties: props, body: body);

        Console.WriteLine($"[RabbitMQ] Published {typeof(T).Name} to exchange: {entityName}");

        foreach (var iface in typeof(T).GetInterfaces())
        {
            var ifaceTopo = PublishTopology.GetMessageTopology(iface);
            foreach (var boundType in ifaceTopo.BoundTypes)
            {
                var boundExchange = MessageTopology.For(boundType).EntityName;
                await channel.ExchangeDeclareAsync(boundExchange, exchangeType, durable: true);
                await channel.ExchangeBindAsync(boundExchange, entityName, "");
            }
        }
    }

    public async Task ReceiveEndpoint<T>(string queueName, MessageHandlerDelegate<T> onMessage)
    {
        var entityName = MessageTopology.For<T>().EntityName;

        var conn = await _factory.CreateConnectionAsync();
        var channel = await conn.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(entityName, "fanout", durable: true);
        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync(queueName, entityName, "");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var msg = JsonSerializer.Deserialize<T>(body);
            await onMessage(new ReceiveContext<T>(msg, ea.BasicProperties.Headers!, ea.CancellationToken));

            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

        Console.WriteLine($"[RabbitMQ] Listening to queue: {queueName} for {typeof(T).Name}");
    }
}