using MyServiceBus.Transport;

namespace MyServiceBus;

public delegate Task ReceiveEndpointHandler<T>(ConsumeContext<T> consumeContext);