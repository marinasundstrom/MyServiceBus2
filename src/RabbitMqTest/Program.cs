using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;

using MyServiceBus;
using MyServiceBus.RabbitMq;
using MyServiceBus.Topology;

ServiceCollection services = new ServiceCollection();
services.AddScoped<OrderSubmittedConsumer>();

var sp = services.BuildServiceProvider();

var topology = new DefaultBusTopology();
//topology.For<MyCommand>().SetEntityName("my-queue");
topology.For<UserCreatedEvent>().SetEntityName("user-created");
topology.Publish<IEvent>().AddMessagePublishTopology<UserCreatedEvent>();

//var bus = new RabbitMqMessageBus(topology, host: "localhost");

var transportFactory = new RabbitMqTransportFactory(topology);

var rec = new ReceiveEndpointConfigurator(
    await transportFactory.CreateReceiveTransport(),
    topology.Consumer(),
    sp);

rec.Consumer<OrderSubmittedConsumer>();

var bus = new MessageBus(topology, transportFactory);

await bus.StartAsync();

await bus.Publish(new OrderSubmitted { OrderId = Guid.NewGuid() });

Console.WriteLine("Waiting for messages. Press Enter to exit.");
Console.ReadLine();

return;

// Configure topology

// Publish
await bus.Publish(new UserCreatedEvent("123"));

// Receive
await bus.ReceiveEndpoint<UserCreatedEvent>("user-event-queue", async context =>
{
    var message = context.Message;

    Console.WriteLine($"[Consumer] Got event: {message.UserId}");

    await Task.CompletedTask;
});

var pipeline = new MessagePipeline<MyCommand>();
pipeline.Use(new LoggingFilter<MyCommand>());
pipeline.Use(new TimingFilter<MyCommand>());

await bus.ReceiveEndpoint<MyCommand>("MyCommand", pipeline.Build(async context =>
{
    var message = context.Message;

    Console.WriteLine($"[Consumer] Received command: {message.CorrelationId}");

    await Task.Delay(1200);
}));

await bus.Send(new MyCommand { CorrelationId = "abc123" });

Console.WriteLine("Waiting for messages. Press Enter to exit.");
Console.ReadLine();

public interface IEvent { }

public record UserCreatedEvent(string UserId) : IEvent;