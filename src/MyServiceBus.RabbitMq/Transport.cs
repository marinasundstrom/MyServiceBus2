using System.Text.Json;

using MyServiceBus.Topology;
using MyServiceBus.Transport;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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
        connection ??= await _factory.CreateConnectionAsync();
        return new RabbitMqTransport(connection, _topology);
    }

    public async Task<IReceiveTransport> CreateReceiveTransport(CancellationToken cancellationToken = default)
    {
        connection ??= await _factory.CreateConnectionAsync();
        return new RabbitMqTransport(connection, _topology);
    }

    public async Task<ISendTransport> CreateSendTransport(CancellationToken cancellationToken = default)
    {
        connection ??= await _factory.CreateConnectionAsync();
        return new RabbitMqTransport(connection, _topology);
    }
}

public class RabbitMqTransport : ISendTransport, IPublishTransport, IReceiveTransport
{
    private readonly IBusTopology _topology;
    private readonly IConnection _connection;

    public RabbitMqTransport(IConnection connection, IBusTopology topology)
    {
        _connection = connection;
        _topology = topology;
    }

    public async Task Send<T>(T message, SendContext context)
    {
        using var ch = await _connection.CreateChannelAsync();

        var queue = _topology.For<T>().EntityName;
        await ch.QueueDeclareAsync(queue, true, false, false);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        var props = new BasicProperties();
        props.CorrelationId = context.CorrelationId;

        await ch.BasicPublishAsync("", queue, false, props, body);
    }

    public async Task Publish<T>(T message, PublishContext context)
    {
        using var ch = await _connection.CreateChannelAsync();

        await ch.ExchangeDeclareAsync(context.ExchangeName, context.ExchangeType, durable: true);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);
        var props = new BasicProperties();

        await ch.BasicPublishAsync(context.ExchangeName, "", false, props, body);
    }

    public async Task Subscribe<T>(string queue, Transport.MessageHandlerDelegate<T> handler)
    {
        var ch = await _connection.CreateChannelAsync();

        var exchange = _topology.For<T>().EntityName;
        await ch.ExchangeDeclareAsync(exchange, "fanout", durable: true);
        await ch.QueueDeclareAsync(queue, true, false, false);
        await ch.QueueBindAsync(queue, exchange, "");

        var consumer = new AsyncEventingBasicConsumer(ch);
        consumer.ReceivedAsync += async (s, ea) =>
        {
            await handler(new ReceiveContext<T>(ea.Body, ea.BasicProperties.Headers!, ea.CancellationToken));
            await ch.BasicAckAsync(ea.DeliveryTag, false);
        };

        await ch.BasicConsumeAsync(queue, autoAck: false, consumer);
    }
}