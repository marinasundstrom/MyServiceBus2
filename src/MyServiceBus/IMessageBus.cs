namespace MyServiceBus;

public interface IMessageBus
{
    Task Publish<T>(T message);
    Task ReceiveEndpoint<T>(string queue, MessageHandlerDelegate<T> handler);
    Task Send<T>(T message);
}
