using System;

namespace MyServiceBus.Topology;

public interface IBusTopology
{
    IMessageTopology For(Type type);
    IMessageTopology<TMessage> For<TMessage>();
    IPublishTopology<TMessage> Publish<TMessage>();
    IPublishTopology Publish(Type messageType);
    ISendTopology<T> Send<T>();
}
