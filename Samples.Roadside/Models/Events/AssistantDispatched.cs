namespace Samples.Roadside.Models.Events;

public record AssistantDispatched
{
    public string AssistantId { get; set; }
    public string CustomerId { get; set; }
    public string DispatchId { get; set; }
    public bool Dispatched { get; set; }
}
