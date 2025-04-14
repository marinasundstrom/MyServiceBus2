namespace MyServiceBus;

public interface ConsumeContext<T>
    where T : class
{
    T Message { get; }

    CancellationToken CancellationToken { get; }
}
