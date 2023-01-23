using Samples.Roadside.Contracts;

namespace Samples.Roadside.Models;

public record ConsumedEvent<T> : IPartitionOffset
{
    public int Partition { get; set; }
    public long Offset { get; set; }
    public T Event { get; set; }
}
