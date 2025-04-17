namespace MyServiceBus.Transports;

public interface ISendTransport
{
    Task Send<T>(SendContext<T> context);
}
