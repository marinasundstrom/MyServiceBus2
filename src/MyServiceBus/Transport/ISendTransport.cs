namespace MyServiceBus.Transport;

public interface ISendTransport
{
    Task Send<T>(T message, SendContext context);
}
