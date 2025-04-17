namespace MyServiceBus.Transports;

public interface IPublishTransport
{
    Task Publish<T>(PublishContext<T> context);
}
