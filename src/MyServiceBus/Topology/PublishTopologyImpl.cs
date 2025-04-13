namespace MyServiceBus.Topology;

public class PublishTopologyImpl<TMessage> : IPublishTopology<TMessage>
{
    public string ExchangeType { get; } = "fanout";

    public bool Exclude { get; set; } = false;

    private readonly List<Type> _boundTypes = new();

    public void AddMessagePublishTopology<TSub>() where TSub : TMessage
    {
        _boundTypes.Add(typeof(TSub));
    }

    public IEnumerable<Type> BoundTypes => _boundTypes;
}
