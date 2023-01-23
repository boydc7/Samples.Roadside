using Samples.Roadside.Models;
using Samples.Roadside.Models.Events;

namespace Samples.Roadside.Contracts;

public interface ILocationEventConsumer
{
    IEnumerable<ConsumedEvent<AssistantLocationUpdate>> Consume();

    IAsyncEnumerable<T> AckConsumesAsync<T>(IAsyncEnumerable<T> messages)
        where T : IPartitionOffset;

    void AckConsume(int partition, long offset);
}
