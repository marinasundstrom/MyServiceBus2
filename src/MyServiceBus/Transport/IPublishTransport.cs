namespace MyServiceBus.Transport;

public interface IPublishTransport
{
    Task Publish<T>(T message, PublishContext context);
}
