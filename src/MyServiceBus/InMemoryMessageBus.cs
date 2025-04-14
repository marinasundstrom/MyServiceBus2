using MyServiceBus;
using MyServiceBus.Topology;

public class InMemoryMessageBus : IMessageBus
{
    private readonly Dictionary<string, List<Func<object, Task>>> _handlers = new();
    private readonly object _lock = new();

    public IBusTopology Topology { get; } = new DefaultBusTopology();

    public Task Send<T>(T message)
    {
        var queueName = Topology.For<T>().EntityName;

        var correlationId = Topology.Send<T>().GetCorrelationId(message);
        Console.WriteLine($"[Send] {typeof(T).Name} with CorrelationId = {correlationId}");

        if (_handlers.TryGetValue(queueName, out var handlerList))
        {
            var tasks = handlerList.Select(h => h(message));
            return Task.WhenAll(tasks);
        }

        return Task.CompletedTask;
    }

    public Task Publish<T>(T message)
    {
        var topology = Topology.Publish<T>();
        if (topology.Exclude)
        {
            Console.WriteLine($"[Publish] Skipped {typeof(T).Name}");
            return Task.CompletedTask;
        }

        var entityName = Topology.For<T>().EntityName;
        Console.WriteLine($"[Publish] Publishing {typeof(T).Name} to Exchange: '{entityName}' (fanout)");

        if (_handlers.TryGetValue(entityName, out var handlerList))
        {
            foreach (var handler in handlerList)
                handler(message);
        }

        // Simulera routed till bound types (t.ex. interfaces)
        foreach (var iface in typeof(T).GetInterfaces())
        {
            var ifaceTopo = Topology.Publish(iface);
            foreach (var boundType in ifaceTopo.BoundTypes)
            {
                var boundExchange = Topology.For(boundType).EntityName;
                Console.WriteLine($"Also routed to bound type exchange: {boundType.Name}");

                if (_handlers.TryGetValue(boundExchange, out var boundList))
                {
                    foreach (var handler in boundList)
                        handler(message);
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task ReceiveEndpoint<T>(string queueName, ReceiveEndpointHandler<T> onMessage)
    {
        lock (_lock)
        {
            if (!_handlers.TryGetValue(queueName, out var list))
                list = _handlers[queueName] = new List<Func<object, Task>>();

            list.Add(msg =>
            {
                if (msg is T typed)
                {
                    return onMessage(new InMemoryConsumeContextImpl<T>(typed));
                }

                return Task.CompletedTask;
            });
        }

        Console.WriteLine($"[InMemoryBus] Subscribed to {typeof(T).Name} on queue '{queueName}'");

        return Task.CompletedTask;
    }
}