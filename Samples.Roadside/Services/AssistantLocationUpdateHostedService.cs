using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Samples.Roadside.Contracts;

namespace Samples.Roadside.Services;

public class AssistantLocationUpdateHostedService : IHostedService, IAsyncDisposable
{
    private readonly ILocationEventConsumer _locationEventConsumer;
    private readonly ILocationService _locationService;
    private readonly ILogger<AssistantLocationUpdateHostedService> _log;
    private readonly TimeSpan _pollingInterval;
    private Timer _timer;
    private readonly object _lockObject = new();
    private bool _inShutdown;
    private bool _inHostProcessing;

    public AssistantLocationUpdateHostedService(ILocationEventConsumer locationEventConsumer,
                                                ILocationService locationService,
                                                ILogger<AssistantLocationUpdateHostedService> log)
    {
        _locationEventConsumer = locationEventConsumer;
        _locationService = locationService;
        _log = log;
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
                await ProcessLocationUpdatesAsync();
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

    public async Task ProcessLocationUpdatesAsync()
    {
        try
        {
            foreach (var locationEvent in _locationEventConsumer.Consume())
            {
                await _locationService.UpdateAssistantLocationAsync(locationEvent.Event.AssistantId,
                                                                    locationEvent.Event.Location);

                _locationEventConsumer.AckConsume(locationEvent.Partition, locationEvent.Offset);
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
