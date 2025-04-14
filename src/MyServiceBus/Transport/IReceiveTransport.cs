namespace MyServiceBus.Transport;

public interface IReceiveTransport
{
    Task Subscribe<T>(string queue, MessageHandlerDelegate<T> handler);
}
