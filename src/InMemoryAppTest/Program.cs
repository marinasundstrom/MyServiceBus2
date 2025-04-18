﻿using MyServiceBus;
using MyServiceBus.Topology;

var bus = new InMemoryMessageBus();

// Configure
bus.Topology.For<MyCommand>().SetEntityName("my-queue");
bus.Topology.For<MyEvent>().SetEntityName("my-event");

await bus.ReceiveEndpoint<MyCommand>("my-queue", context =>
{
    var message = context.Message;

    Console.WriteLine($"[Consumer] Received command: {message.CorrelationId}");
    return Task.CompletedTask;
});

await bus.ReceiveEndpoint<MyEvent>("my-event", context =>
{
    var message = context.Message;

    Console.WriteLine($"[Consumer] Received event: {message.Name}");
    return Task.CompletedTask;
});

// Act
await bus.Send(new MyCommand { CorrelationId = "abc123" });
await bus.Publish(new MyEvent { Name = "HelloEvent" });

public class MyEvent
{
    public string Name { get; set; }
}

public record MyMessage
{

}

public record MyCommand
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}
