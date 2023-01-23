namespace Samples.Roadside.Contracts;

public interface IPartitionOffset
{
    public int Partition { get; set; }
    public long Offset { get; set; }
}
