using System;

using MyServiceBus.Transport;

namespace MyServiceBus;

public interface IConsumeFilter<T>
{
    Task Send(ConsumeContext<T> context, ReceiveEndpointHandler<T> next);
}
