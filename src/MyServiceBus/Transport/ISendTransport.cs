namespace MyServiceBus.Transport;

public interface ISendTransport
{
    Task Send<T>(SendContext<T> context);
}
