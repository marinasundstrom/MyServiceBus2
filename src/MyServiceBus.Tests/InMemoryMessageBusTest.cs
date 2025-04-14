namespace MyServiceBus.Tests;

using Xunit;
using System;
using System.IO;
using System.Threading.Tasks;
using MyServiceBus.Topology;

public class InMemoryMessageBusTest
{
    [Fact]
    public async Task Publish_ShouldUseDefaultExchangeName_WhenNoneIsConfigured()
    {
        var output = new StringWriter();
        Console.SetOut(output);

        var bus = new InMemoryMessageBus();

        await bus.Publish(new MyMessage());

        var result = output.ToString();
        Assert.Contains("Exchange: 'MyMessage'", result); // assuming default naming convention
    }

    [Fact]
    public async Task ReceiveEndpoint_ShouldInvokeHandler1()
    {
        var output = new StringWriter();
        Console.SetOut(output);

        var bus = new InMemoryMessageBus();

        //bus.Topology.For<UserCreatedEvent>();

        await bus.ReceiveEndpoint<UserCreatedEvent>("UserCreatedEvent", context =>
            {
                Console.WriteLine($"[Handler] Received UserCreatedEvent with UserId = {context.Message.UserId}");

                return Task.CompletedTask;
            });

        await bus.Publish(new UserCreatedEvent("456"));

        var result = output.ToString();
        Assert.Contains("[Handler] Received UserCreatedEvent with UserId = 456", result);
    }

    [Fact]
    public async Task ReceiveEndpoint_ShouldInvokeHandler2()
    {
        var output = new StringWriter();
        Console.SetOut(output);

        var bus = new InMemoryMessageBus();

        bus.Topology.For<UserCreatedEvent>().SetEntityName("user-created-queue");

        await bus.ReceiveEndpoint<UserCreatedEvent>("user-created-queue", context =>
            {
                Console.WriteLine($"[Handler] Received UserCreatedEvent with UserId = {context.Message.UserId}");

                return Task.CompletedTask;
            });

        await bus.Publish(new UserCreatedEvent("456"));

        var result = output.ToString();
        Assert.Contains("[Handler] Received UserCreatedEvent with UserId = 456", result);
    }

    [Fact]
    public async Task Send_ShouldUseConfiguredCorrelationId()
    {
        var output = new StringWriter();
        Console.SetOut(output);

        var bus = new InMemoryMessageBus();
        var message = new MyCommand();

        bus.Topology.Send<MyCommand>().UseCorrelationId(x => x.CorrelationId);

        await bus.Send(message);

        var result = output.ToString();
        Assert.Contains($"[Send] MyCommand with CorrelationId = {message.CorrelationId}", result);
    }

    [Fact]
    public async Task Publish_ShouldRespectExcludeFlag()
    {
        var output = new StringWriter();
        Console.SetOut(output);

        var bus = new InMemoryMessageBus();

        var topology = (PublishTopologyImpl<MyMessage>)bus.Topology.Publish<MyMessage>();
        topology.Exclude = true;

        await bus.Publish(new MyMessage());

        var result = output.ToString();
        Assert.Contains("[Publish] Skipped MyMessage", result);
    }

    [Fact]
    public async Task Publish_ShouldUseMessageEntityName()
    {
        var output = new StringWriter();
        Console.SetOut(output);

        var bus = new InMemoryMessageBus();
        bus.Topology.For<MyMessage>().SetEntityName("my-message-exchange");

        await bus.Publish(new MyMessage());

        var result = output.ToString();
        Assert.Contains("[Publish] Publishing MyMessage to Exchange: 'my-message-exchange' (fanout)", result);
    }

    [Fact]
    public async Task Publish_ShouldIncludeBoundTypes()
    {
        var output = new StringWriter();
        Console.SetOut(output);

        var bus = new InMemoryMessageBus();

        var pubTopology = (PublishTopologyImpl<IEvent>)bus.Topology.Publish(typeof(IEvent));
        pubTopology.AddMessagePublishTopology<UserCreatedEvent>();

        bus.Topology.For<UserCreatedEvent>().SetEntityName("user-created");

        await bus.Publish(new UserCreatedEvent("123"));

        var result = output.ToString();
        Assert.Contains("Publishing UserCreatedEvent", result);
        Assert.Contains("Also routed to bound type exchange: UserCreatedEvent", result);
    }

    [Fact]
    public async Task Send_ShouldUseLatestCorrelationIdSelector()
    {
        var output = new StringWriter();
        Console.SetOut(output);

        var bus = new InMemoryMessageBus();
        var message = new MyCommand { CorrelationId = Guid.NewGuid().ToString() };

        bus.Topology.Send<MyCommand>().UseCorrelationId(x => Guid.Empty.ToString()); // dummy
        bus.Topology.Send<MyCommand>().UseCorrelationId(x => x.CorrelationId); // should override

        await bus.Send(message);

        var result = output.ToString();
        Assert.Contains(message.CorrelationId.ToString(), result);
    }

    [Fact(Skip = "Re-visit")]
    public async Task Publish_InterfaceWithMultipleImplementations_ShouldNotDuplicate()
    {
        var output = new StringWriter();
        Console.SetOut(output);

        var bus = new InMemoryMessageBus();

        var pubTopology = (PublishTopologyImpl<IEvent>)bus.Topology.Publish(typeof(IEvent));
        pubTopology.AddMessagePublishTopology<UserCreatedEvent>();
        pubTopology.AddMessagePublishTopology<AccountCreatedEvent>(); // another impl

        await bus.Publish(new AccountCreatedEvent("acc"));

        var result = output.ToString();
        Assert.Contains("Publishing AccountCreatedEvent", result);
        Assert.Contains("Also routed to bound type exchange: AccountCreatedEvent", result);
        Assert.DoesNotContain("UserCreatedEvent", result); // sanity check
    }
}

public record AccountCreatedEvent(string UserName) : IEvent;