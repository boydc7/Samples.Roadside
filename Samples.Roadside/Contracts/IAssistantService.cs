using Samples.Roadside.Models;

namespace Samples.Roadside.Contracts;

public interface IAssistantService
{
    Task<Assistant> GetByIdAsync(string id);
    Task<IReadOnlyList<Assistant>> GetManyAsync(int limit = 500);
    Task InsertAsync(Assistant customer);
    Task UpdateAsync(Assistant customer);
    Task<IReadOnlyList<Assistant>> GetByIdsAsync(IEnumerable<string> ids);
}
