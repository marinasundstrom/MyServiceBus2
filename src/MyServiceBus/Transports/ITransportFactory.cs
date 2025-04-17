namespace MyServiceBus.Transports;

public interface ITransportFactory
{
    Task<ISendTransport> CreateSendTransport(CancellationToken cancellationToken = default);

    Task<IPublishTransport> CreatePublishTransport(CancellationToken cancellationToken = default);

    Task<IReceiveTransport> CreateReceiveTransport(CancellationToken cancellationToken = default);
}