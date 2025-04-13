namespace MyServiceBus.Transport;

public interface ITransport
{
    Task Send<T>(T message, SendContext context);
    Task Publish<T>(T message, PublishContext context);
    Task Subscribe<T>(string queue, MessageHandlerDelegate<T> handler);
}
