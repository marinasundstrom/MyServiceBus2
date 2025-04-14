namespace MyServiceBus;

public class MessagePipeline<T>
{
    private readonly List<IConsumeFilter<T>> _filters = new();

    public void Use(IConsumeFilter<T> filter)
    {
        _filters.Add(filter);
    }

    public ReceiveEndpointHandler<T> Build(ReceiveEndpointHandler<T> terminal)
    {
        ReceiveEndpointHandler<T> current = terminal;

        foreach (var filter in _filters.AsEnumerable().Reverse())
        {
            var next = current;
            current = ctx => filter.Send(ctx, next);
        }

        return current;
    }
}