using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Samples.Roadside.Contracts;

namespace Samples.Roadside.Services;

public class AssistantDispatchHostedService : IHostedService, IAsyncDisposable
{
    private readonly IDispatchEventConsumer _dispatchEventConsumer;
    private readonly ILogger<AssistantDispatchHostedService> _log;
    private readonly IReadOnlyList<IDispatchUpdateService> _dispatchUpdateServices;
    private readonly TimeSpan _pollingInterval;
    private Timer _timer;
    private readonly object _lockObject = new();
    private bool _inShutdown;
    private bool _inHostProcessing;

    public AssistantDispatchHostedService(IDispatchEventConsumer dispatchEventConsumer,
                                          ILogger<AssistantDispatchHostedService> log,
                                          IEnumerable<IDispatchUpdateService> dispatchUpdateServices)
    {
        _dispatchEventConsumer = dispatchEventConsumer;
        _log = log;
        _dispatchUpdateServices = dispatchUpdateServices.AsListReadOnly();
        _pollingInterval = TimeSpan.FromSeconds(3);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(ProcessTimer, null, _pollingInterval, _pollingInterval);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _inShutdown = true;

        try
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            await _timer.DisposeAsync();

            _timer = null;

            // Attemp to let the host gracefully stop
            while (_inHostProcessing)
            {
                await Task.Delay(500, cancellationToken);
            }
        }
        catch
        {
            // best attempt
        }
    }

    private void ProcessTimer(object state)
    {
        if (_inHostProcessing || _inShutdown)
        {
            return;
        }

#pragma warning disable 4014
        ProcessTimerAsync();
#pragma warning restore 4014
    }

    private async Task ProcessTimerAsync()
    {
        if (_inHostProcessing || _inShutdown)
        {
            return;
        }

        lock(_lockObject)
        {
            if (_inHostProcessing || _inShutdown)
            {
                return;
            }

            _inHostProcessing = true;
        }

        try
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            if (!_inShutdown)
            {
                await ProcessDispatchesAsync();
            }
        }
        finally
        {
            if (!_inShutdown)
            {
                _timer.Change(_pollingInterval, _pollingInterval);
            }

            _inHostProcessing = false;
        }
    }

    public async Task ProcessDispatchesAsync()
    {
        try
        {
            foreach (var dispatchEvent in _dispatchEventConsumer.Consume())
            {
                var tasks = _dispatchUpdateServices.Select(s => s.ProcessDispatchUpdateAsync(dispatchEvent.Event.AssistantId,
                                                                                             dispatchEvent.Event.Dispatched))
                                                   .ToArray();

                await Task.WhenAll(tasks);

                _dispatchEventConsumer.AckConsume(dispatchEvent.Partition, dispatchEvent.Offset);
            }
        }
        catch(TaskCanceledException) { }
        catch(Exception x)
        {
            _log.Exception(x);
        }
        finally
        {
            _log.LogInformation("Ending AssistantLocationUpdate stream processing");
        }
    }

    public void Dispose()
        => DisposeAsync().AsTask().GetAwaiter().GetResult();

    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None);

        GC.SuppressFinalize(this);
    }
}
