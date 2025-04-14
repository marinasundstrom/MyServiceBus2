namespace MyServiceBus.Transport;

public interface IPublishTransport
{
    Task Publish<T>(PublishContext<T> context);
}
