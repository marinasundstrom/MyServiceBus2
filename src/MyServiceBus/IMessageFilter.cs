using System;
using MyServiceBus.Transport;

namespace MyServiceBus;

public interface IMessageFilter<T>
{
    Task Send(ReceiveContext<T> context, MessageHandlerDelegate<T> next);
}
