namespace MyServiceBus.Topology;

public interface IMessageTopology
{
    string EntityName { get; }
    bool IsTemporary { get; }
}

public interface IMessageTopology<TMessage> : IMessageTopology
{
    void SetEntityName(string EntityName);
}
