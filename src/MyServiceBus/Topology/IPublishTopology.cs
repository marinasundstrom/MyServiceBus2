namespace MyServiceBus.Topology;

public interface IPublishTopology
{
    IEnumerable<Type> BoundTypes { get; }
}

public interface IPublishTopology<TMessage> : IPublishTopology
{
    string ExchangeType { get; }
    bool Exclude { get; set; }

    void AddMessagePublishTopology<TSub>() where TSub : TMessage;
}
