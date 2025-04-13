namespace MyServiceBus.Topology;

public interface IConsumerTopology
{
    bool ConfigureConsumeTopology { get; }

    void Bind<TMessage>();
}