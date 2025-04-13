using MyServiceBus;
using MyServiceBus.Topology;
using MyServiceBus.Transport;

public class InMemoryMessageBus : IMessageBus
{
    private readonly Dictionary<string, List<Func<object, Task>>> _handlers = new();
    private readonly object _lock = new();

    public Task Send<T>(T message)
    {
        var queueName = MessageTopology.For<T>().EntityName;

        var correlationId = SendTopology.Send<T>().GetCorrelationId(message);
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
        var topology = PublishTopology.GetMessageTopology<T>();
        if (topology.Exclude)
        {
            Console.WriteLine($"[Publish] Skipped {typeof(T).Name}");
            return Task.CompletedTask;
        }

        var entityName = MessageTopology.For<T>().EntityName;
        Console.WriteLine($"[Publish] Publishing {typeof(T).Name} to Exchange: '{entityName}' (fanout)");

        if (_handlers.TryGetValue(entityName, out var handlerList))
        {
            foreach (var handler in handlerList)
                handler(message);
        }

        // Simulera routed till bound types (t.ex. interfaces)
        foreach (var iface in typeof(T).GetInterfaces())
        {
            var ifaceTopo = PublishTopology.GetMessageTopology(iface);
            foreach (var boundType in ifaceTopo.BoundTypes)
            {
                var boundExchange = MessageTopology.For(boundType).EntityName;
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

    public Task ReceiveEndpoint<T>(string queueName, MessageHandlerDelegate<T> onMessage)
    {
        lock (_lock)
        {
            if (!_handlers.TryGetValue(queueName, out var list))
                list = _handlers[queueName] = new List<Func<object, Task>>();

            list.Add(msg =>
            {
                if (msg is T typed)
                    return onMessage(new ReceiveContext<T>(typed, new Dictionary<string, object?>(), CancellationToken.None));

                return Task.CompletedTask;
            });
        }

        Console.WriteLine($"[InMemoryBus] Subscribed to {typeof(T).Name} on queue '{queueName}'");

        return Task.CompletedTask;
    }
}