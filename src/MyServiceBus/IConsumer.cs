namespace MyServiceBus.Topology;

public interface IConsumer<TMessage>
    where TMessage : class
{
    Task Consume(ConsumeContext<TMessage> context);
}