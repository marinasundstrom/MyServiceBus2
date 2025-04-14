namespace MyServiceBus.Topology;

public class ConsumeTopologyImpl : IConsumeTopology
{
    private readonly IBusTopology _busTopology;

    public bool ConfigureConsumeTopology { get; }

    private readonly List<BindingInfo> _bindings = new();

    public ConsumeTopologyImpl(IBusTopology busTopology, bool configureConsumeTopology = true)
    {
        _busTopology = busTopology;
        ConfigureConsumeTopology = configureConsumeTopology;
    }

    public void Bind<TMessage>()
    {
        if (!ConfigureConsumeTopology)
            return;

        var publishTopology = _busTopology.Publish<TMessage>();

        if (publishTopology is PublishTopologyImpl<TMessage> impl)
        {
            var exchange = typeof(TMessage).Name;
            _bindings.Add(new BindingInfo(exchange, impl.ExchangeType));

            foreach (var boundType in impl.BoundTypes)
            {
                _bindings.Add(new BindingInfo(boundType.Name, impl.ExchangeType));
            }
        }
    }

    public IEnumerable<BindingInfo> GetBindings() => _bindings;

    public record BindingInfo(string ExchangeName, string ExchangeType);
}