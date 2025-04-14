namespace MyServiceBus;

public class InMemoryConsumeContextImpl<T> : ConsumeContext<T>
    where T : class
{
    public InMemoryConsumeContextImpl(T message)
    {
        Message = message;
    }

    public T Message { get; }

    public CancellationToken CancellationToken { get; }
}
