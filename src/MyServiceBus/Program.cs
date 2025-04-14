using MyServiceBus;
using MyServiceBus.Topology;

public class OrderSubmittedConsumer : IConsumer<OrderSubmitted>
{
    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        Console.WriteLine($"Handled OrderSubmitted: {context.Message.OrderId}");
        return Task.CompletedTask;
    }
}

public record OrderSubmitted
{
    public Guid OrderId { get; init; }
}

public record MyMessage
{

}

public record MyCommand
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}
