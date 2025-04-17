using MyServiceBus.Transports;

namespace MyServiceBus;

public delegate Task ReceiveEndpointHandler<T>(ConsumeContext<T> consumeContext)
    where T : class;