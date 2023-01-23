using Samples.Roadside.Models.Events;

namespace Samples.Roadside.Contracts;

public interface IDispatchEventProducer
{
    Task ProduceAsync(AssistantDispatched message);
}

public interface ILocationEventProducer
{
    Task ProduceAsync(AssistantLocationUpdate message);
}
