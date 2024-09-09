using System;
using System.Threading;
using System.Threading.Tasks;
using DataTransfer.Models;
using Microsoft.Extensions.Hosting;

public class TimedHostedService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly IDataTransferService _dataTransferService;

    public TimedHostedService(IDataTransferService dataTransferService)
    {
        _dataTransferService = dataTransferService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Run the task immediately and then every hour
        _timer = new Timer(
            async (state) => await DoWork(),
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(50));

        return Task.CompletedTask;
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
