using System;

namespace MyServiceBus.Transports;

public delegate Task ReceiveHandler<T>(ReceiveContext<T> context)
    where T : class;
