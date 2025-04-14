using System;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using MyServiceBus.Transport;

namespace MyServiceBus.Topology;

public class ReceiveEndpointConfigurator
{
    private readonly IReceiveTransport _receiveTransport;
    private readonly IConsumeTopology _consumeTopology;
    private readonly IServiceProvider _provider;

    public ReceiveEndpointConfigurator(
        IReceiveTransport receiveTransport,
        IConsumeTopology consumeTopology,
        IServiceProvider provider)
    {
        _receiveTransport = receiveTransport;
        _consumeTopology = consumeTopology;
        _provider = provider;
    }

    public void Consumer<TConsumer>() where TConsumer : class
    {
        var consumerType = typeof(TConsumer);
        var messageTypes = GetHandledMessageTypes(consumerType);

        foreach (var messageType in messageTypes)
        {
            var method = typeof(ReceiveEndpointConfigurator)
                .GetMethod(nameof(RegisterHandler), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(consumerType, messageType);

            method.Invoke(this, null);
        }
    }

    private string GetQueueName<TConsumer, TMessage>() =>
        $"{typeof(TConsumer).Name}_{typeof(TMessage).Name}";

    private void RegisterHandler<TConsumer, TMessage>()
        where TConsumer : class, IConsumer<TMessage>
        where TMessage : class
    {
        _consumeTopology.Bind<TMessage>();

        string queueName = GetQueueName<TConsumer, TMessage>();

        _receiveTransport.Subscribe<TMessage>(queueName, async context =>
        {
            using var scope = _provider.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();
            var consumeContext = new ConsumeContextImpl<TMessage>(context);
            await consumer.Consume(consumeContext);
        });
    }

    public static IEnumerable<Type> GetHandledMessageTypes(Type consumerType)
    {
        return consumerType
            .GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConsumer<>))
            .Select(i => i.GetGenericArguments()[0]);
    }
}
