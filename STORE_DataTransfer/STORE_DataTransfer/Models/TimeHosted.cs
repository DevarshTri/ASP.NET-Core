using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using STORE_DataTransfer.Models;

public class TimedHostedService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly IDataTransferService _dataTransferService;
    private bool _isRunning;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public TimedHostedService(IDataTransferService dataTransferService)
    {
        _dataTransferService = dataTransferService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Run the task immediately and then every interval
        _timer = new Timer(
            async (state) => await ExecuteTaskAsync(),
            null,
            TimeSpan.Zero,
            _interval);

        return Task.CompletedTask;
    }

    private async Task ExecuteTaskAsync()
    {
        if (_isRunning)
        {
            // Skip this execution if the previous task is still running
            return;
        }

        try
        {
            _isRunning = true;
            await DoWork();
        }
        finally
        {
            _isRunning = false;
        }
    }

    private async Task DoWork()
    {
        await _dataTransferService.TransferDataAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
