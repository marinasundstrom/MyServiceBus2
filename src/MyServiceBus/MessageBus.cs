using MyServiceBus.Transport;

namespace MyServiceBus;

public class MessageBus : IMessageBus
{
    private readonly ITransport _transport;

    public MessageBus(ITransport transport)
    {
        _transport = transport;
    }

    public Task Send<T>(T message)
    {
        var context = new SendContext
        {
            CorrelationId = ((SendTopologyImpl<T>)SendTopology.Send<T>()).GetCorrelationId(message)
        };

        return _transport.Send(message, context);
    }

    public Task Publish<T>(T message)
    {
        var pubTopology = PublishTopology.GetMessageTopology<T>();
        if (pubTopology.Exclude)
            return Task.CompletedTask;

        var context = new PublishContext
        {
            ExchangeName = MessageTopology.For<T>().EntityName,
            ExchangeType = pubTopology.ExchangeType
        };

        return _transport.Publish(message, context);
    }

    public Task ReceiveEndpoint<T>(string queue, MessageHandlerDelegate<T> handler)
    {
        return _transport.Subscribe(queue, handler);
    }
}
