using System;
using MyServiceBus.Transport;

namespace MyServiceBus;

public delegate Task MessageHandlerDelegate<T>(ReceiveContext<T> context);
