namespace Samples.Roadside.Configuration;

public class SampleShutdownCancellationSource
{
    private SampleShutdownCancellationSource() { }

    public static readonly SampleShutdownCancellationSource Instance = new();

    public void TryCancel()
    {
        try
        {
            CancellationTokenSource.Cancel();
        }
        catch
        {
            // Ignore - shuting down
        }
    }

    public CancellationTokenSource CancellationTokenSource { get; } = new();
    public CancellationToken Token => CancellationTokenSource.Token;
}
