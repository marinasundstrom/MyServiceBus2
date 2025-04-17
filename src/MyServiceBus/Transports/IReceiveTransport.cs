using System.Globalization;

namespace MyServiceBus.Transports;

public interface IReceiveTransport
{
    Task Subscribe<T>(string queue, ReceiveHandler<T> handler)
        where T : class;
}
