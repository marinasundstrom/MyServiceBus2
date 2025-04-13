namespace MyServiceBus.Tests;

using Xunit;
using System;
using System.IO;
using System.Threading.Tasks;
using MyServiceBus.Topology;

public class InMemoryMessageBusTest
{
    [Fact]
    public async Task Send_ShouldUseConfiguredCorrelationId()
    {
        var output = new StringWriter();
        Console.SetOut(output);

        var bus = new InMemoryMessageBus();
        var message = new MyCommand();

        SendTopology.Send<MyCommand>().UseCorrelationId(x => x.CorrelationId);

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

        var topology = (PublishTopologyImpl<MyMessage>)PublishTopology.GetMessageTopology<MyMessage>();
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
        MessageTopology.For<MyMessage>().SetEntityName("my-message-exchange");

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

        var pubTopology = (PublishTopologyImpl<IEvent>)PublishTopology.GetMessageTopology(typeof(IEvent));
        pubTopology.AddMessagePublishTopology<UserCreatedEvent>();

        MessageTopology.For<UserCreatedEvent>().SetEntityName("user-created");

        await bus.Publish(new UserCreatedEvent("123"));

        var result = output.ToString();
        Assert.Contains("Publishing UserCreatedEvent", result);
        Assert.Contains("Also routed to bound type exchange: UserCreatedEvent", result);
    }
}