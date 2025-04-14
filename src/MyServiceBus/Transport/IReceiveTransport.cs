namespace MyServiceBus.Transport;

public interface IReceiveTransport
{
    Task Subscribe<T>(string queue, ReceiveHandler<T> handler);
}
