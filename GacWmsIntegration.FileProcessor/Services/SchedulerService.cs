using AutoMapper;
using Cronos;
using GacWmsIntegration.FileProcessor.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace GacWmsIntegration.FileProcessor.Services
{
    public class SchedulerService : BackgroundService
    {
        private readonly ILogger<SchedulerService> _logger;
        private readonly FileProcessingConfig _config;
        private readonly FileProcessingService _fileProcessingService;
        private readonly Dictionary<string, Timer> _timers = new();

        public SchedulerService(
            ILogger<SchedulerService> logger,
            IOptions<FileProcessingConfig> config,
            FileProcessingService fileProcessingService)
        {
            _logger = logger;
            _config = config.Value;
            _fileProcessingService = fileProcessingService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduler Service starting");

            foreach (var watcher in _config.FileWatchers)
            {
                ScheduleFileWatcher(watcher, stoppingToken);
            }

            return Task.CompletedTask;
        }

        private void ScheduleFileWatcher(FileWatcherConfig watcher, CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Scheduling file watcher {WatcherName} with CRON {CronSchedule}",
                    watcher.Name, watcher.CronSchedule);

                var cronExpression = Cronos.CronExpression.Parse(watcher.CronSchedule, CronFormat.Standard);

                var nextOccurrence = cronExpression.GetNextOccurrence(DateTime.UtcNow);

                if (!nextOccurrence.HasValue)
                {
                    _logger.LogWarning("Could not determine next occurrence for CRON expression {CronSchedule}",
                        watcher.CronSchedule);
                    return;
                }

                var delay = nextOccurrence.Value - DateTime.UtcNow;
                if (delay.TotalMilliseconds <= 0)
                {
                    delay = TimeSpan.Zero;
                }

                _logger.LogInformation("Next execution of {WatcherName} scheduled at {NextExecution} (in {Delay})",
                    watcher.Name, nextOccurrence.Value, delay);

                var timer = new Timer(async state =>
                {
                    var watcherConfig = (FileWatcherConfig)state!;

                    try
                    {
                        await _fileProcessingService.ProcessFilesAsync(watcherConfig, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing scheduled task for {WatcherName}", watcherConfig.Name);
                    }
                    finally
                    {
                        // Reschedule the timer for the next occurrence
                        if (!stoppingToken.IsCancellationRequested)
                        {
                            ScheduleFileWatcher(watcherConfig, stoppingToken);
                        }
                    }
                }, watcher, delay, Timeout.InfiniteTimeSpan);

                // Store the timer to prevent garbage collection
                _timers[watcher.Name] = timer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling file watcher {WatcherName}", watcher.Name);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Scheduler Service stopping");

            foreach (var timer in _timers.Values)
            {
                timer.Dispose();
            }
            _timers.Clear();

            return base.StopAsync(cancellationToken);
        }
    }
}
