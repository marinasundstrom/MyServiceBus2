using System;

using MyServiceBus.Transport;

namespace MyServiceBus;

public interface IConsumeFilter<T>
    where T : class
{
    Task Send(ConsumeContext<T> context, ReceiveEndpointHandler<T> next);
}
