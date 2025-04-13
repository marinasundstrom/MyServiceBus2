namespace MyServiceBus;

public class MessagePipeline<T>
{
    private readonly List<IMessageFilter<T>> _filters = new();

    public void Use(IMessageFilter<T> filter)
    {
        _filters.Add(filter);
    }

    public MessageHandlerDelegate<T> Build(MessageHandlerDelegate<T> terminal)
    {
        MessageHandlerDelegate<T> current = terminal;

        foreach (var filter in _filters.AsEnumerable().Reverse())
        {
            var next = current;
            current = ctx => filter.Send(ctx, next);
        }

        return current;
    }
}
