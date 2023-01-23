namespace Samples.Roadside.Contracts;

public interface IDispatchService
{
    Task<(bool Success, string DispatchId)> TryDispatchAssistantAsync(string assistantId, string toCustomerId);
    Task ReleaseAssistantAsync(string assistantId);
}
