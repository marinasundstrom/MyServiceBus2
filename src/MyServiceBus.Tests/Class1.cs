using System;

namespace MyServiceBus.Tests;

public interface IEvent { }

public record MyMessage;

public record MyCommand
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}

public record UserCreatedEvent(string UserId) : IEvent;