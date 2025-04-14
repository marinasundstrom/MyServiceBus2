namespace MyServiceBus;

public interface ConsumeContext<T>
{
    T Message { get; }

    CancellationToken CancellationToken { get; }
}
