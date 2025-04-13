namespace MyServiceBus.Topology;

public class ReceiveEndpointConfigurator : IConsumerTopology
{
    public bool ConfigureConsumeTopology { get; set; } = true;

    public void Bind<TMessage>()
    {
        Console.WriteLine($"Manually binding to {typeof(TMessage).Name}");
    }
}
