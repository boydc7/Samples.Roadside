using Samples.Roadside.Models;
using Samples.Roadside.Models.Events;

namespace Samples.Roadside.Contracts;

public interface IDispatchEventConsumer
{
    IEnumerable<ConsumedEvent<AssistantDispatched>> Consume();

    IAsyncEnumerable<T> AckConsumesAsync<T>(IAsyncEnumerable<T> messages)
        where T : IPartitionOffset;
    
    void AckConsume(int partition, long offset);
}
