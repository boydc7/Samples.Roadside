namespace Samples.Roadside.Contracts;

public interface IDispatchUpdateService
{
    Task ProcessDispatchUpdateAsync(string assistantId, bool isDispatched);
}
