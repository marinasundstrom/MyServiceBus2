namespace MyServiceBus.Topology;

public interface IConsumeTopology
{
    bool ConfigureConsumeTopology { get; }

    void Bind<TMessage>();
}