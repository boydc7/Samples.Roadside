namespace Samples.Roadside.Contracts;

public interface IMigrateData
{
    Task<bool> MigrateAsync(IServiceProvider serviceProvider);
}
