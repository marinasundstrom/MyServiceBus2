using System;

namespace MyServiceBus.Transport;

public delegate Task ReceiveHandler<T>(ReceiveContext<T> context);
