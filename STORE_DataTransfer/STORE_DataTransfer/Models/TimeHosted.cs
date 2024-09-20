using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.Extensions.Hosting;
using STORE_DataTransfer.Models;

public class TimedHostedService : IHostedService, IDisposable
{
    private static readonly ILog log = LogManager.GetLogger(typeof(TimedHostedService));
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
        log.Info("Timed Hosted Service is starting.");
        // Run the task immediately and then every interval
        _timer = new Timer(
            async (state) => await ExecuteTaskAsync(),
            null,
            TimeSpan.Zero,
            _interval);
        log.Info($"Timer set to execute every {_interval.TotalHours} hour(s).");
        return Task.CompletedTask;
    }

    private async Task ExecuteTaskAsync()
    {
        if (_isRunning)
        {
            log.Warn("Data transfer is already running. Skipping this execution.");
            return;
        }

        try
        {
            _isRunning = true;
            log.Info("Data transfer task started.");
            await DoWork();
        }
        catch (Exception ex)
        {
            log.Error("An error occurred while executing the data transfer task.", ex);
        }
        finally
        {
            _isRunning = false;
            log.Info("Data transfer task completed.");
        }
    }

    private async Task DoWork()
    {
        log.Info("Initiating data transfer...");
        await _dataTransferService.TransferDataAsync();
        log.Info("Data transfer finished.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        log.Info("Timed Hosted Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        log.Info("Timed Hosted Service is disposing resources.");
        _timer?.Dispose();
    }
}
