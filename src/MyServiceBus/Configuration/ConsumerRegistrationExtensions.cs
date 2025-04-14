using System.Reflection;

namespace MyServiceBus.Topology;

public static class ConsumerRegistrationExtensions
{
    public static void RegisterAllConsumers(this ReceiveEndpointConfigurator configurator, Assembly assembly)
    {
        var consumerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConsumer<>)));

        foreach (var consumerType in consumerTypes)
        {
            var method = typeof(ReceiveEndpointConfigurator)
                .GetMethod(nameof(ReceiveEndpointConfigurator.Consumer))!
                .MakeGenericMethod(consumerType);

            method.Invoke(configurator, null);
        }
    }
}