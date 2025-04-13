using System.Diagnostics;
using MyServiceBus;
using MyServiceBus.RabbitMq;

//var bus = new RabbitMqMessageBus();
var bus = new MessageBus(new RabbitMqTransport());

// Configure topology
//MessageTopology.For<MyCommand>().SetEntityName("my-queue");
MessageTopology.For<UserCreatedEvent>().SetEntityName("user-created");
PublishTopology.GetMessageTopology<IEvent>().AddMessagePublishTopology<UserCreatedEvent>();

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
    await Task.CompletedTask;
}));

await bus.Send(new MyCommand { CorrelationId = "abc123" });

Console.WriteLine("Waiting for messages. Press Enter to exit.");
Console.ReadLine();

public interface IEvent { }

public record UserCreatedEvent(string UserId) : IEvent;