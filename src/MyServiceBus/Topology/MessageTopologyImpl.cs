namespace MyServiceBus.Topology;

public class MessageTopologyImpl<TMessage> : IMessageTopology<TMessage>
{
    public string EntityName { get; private set; } = typeof(TMessage).Name;
    public bool IsTemporary { get; private set; } = false;

    public void SetEntityName(string entityName)
    {
        EntityName = entityName;
    }
}
