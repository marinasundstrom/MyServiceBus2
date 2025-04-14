using System;

namespace MyServiceBus.Transport;

public delegate Task MessageHandlerDelegate<T>(ReceiveContext<T> context);
