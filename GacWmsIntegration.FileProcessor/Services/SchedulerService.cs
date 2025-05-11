using Cronos;
using GacWmsIntegration.FileProcessor.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GacWmsIntegration.FileProcessor.Services
{
    public class SchedulerService : BackgroundService
    {
        private readonly ILogger<SchedulerService> _logger;
        private readonly FileProcessingConfig _config;
        private readonly FileProcessingService _fileProcessingService;
        private readonly Dictionary<string, Timer> _timers = new();
        private bool _isProcessingEnabled = false;
        private readonly SemaphoreSlim _startSemaphore = new SemaphoreSlim(0, 1);

        public SchedulerService(
            ILogger<SchedulerService> logger,
            IOptions<FileProcessingConfig> config,
            FileProcessingService fileProcessingService)
        {
            _logger = logger;
            _config = config.Value;
            _fileProcessingService = fileProcessingService;
        }

        public async Task StartProcessing()
        {
            _isProcessingEnabled = true;

            // Release the semaphore to allow the ExecuteAsync method to proceed
            if (_startSemaphore.CurrentCount == 0)
            {
                _startSemaphore.Release();
                _logger.LogInformation("Semaphore released to start file processing.");
            }

            _logger.LogInformation("File processing has been enabled");

            // Process files immediately
            if (_config.FileWatchers != null && _config.FileWatchers.Length > 0)
            {
                _logger.LogInformation("Running immediate file processing for all watchers");
                foreach (var watcher in _config.FileWatchers)
                {
                    try
                    {
                        _logger.LogInformation("Immediate processing for watcher: {WatcherName}", watcher.Name);
                        await _fileProcessingService.ProcessFilesAsync(watcher, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during immediate processing for watcher: {WatcherName}", watcher.Name);
                    }
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduler Service is waiting for API to be available...");

            // Wait for the semaphore to be released by the health check service
            await _startSemaphore.WaitAsync(stoppingToken);

            _logger.LogInformation("Scheduler Service starting file processing");

            // Log the configuration to help with debugging
            if (_config.FileWatchers == null || _config.FileWatchers.Length == 0)
            {
                _logger.LogWarning("No file watchers configured in settings. Check your appsettings.json file.");
            }
            else
            {
                _logger.LogInformation("Configured file watchers:");
                foreach (var watcher in _config.FileWatchers)
                {
                    _logger.LogInformation("Watcher: {Name}, Directory: {Directory}, Pattern: {Pattern}, FileType: {FileType}",
                        watcher.Name, watcher.DirectoryPath, watcher.FilePattern, watcher.FileType);

                    // Check if directory exists
                    if (!Directory.Exists(watcher.DirectoryPath))
                    {
                        _logger.LogWarning("Directory does not exist: {Directory}", watcher.DirectoryPath);
                    }
                    else
                    {
                        // Check if there are any matching files
                        var files = Directory.GetFiles(watcher.DirectoryPath, watcher.FilePattern);
                        _logger.LogInformation("Found {Count} files matching pattern in {Directory}",
                            files.Length, watcher.DirectoryPath);
                    }

                    // Schedule the file watcher
                    ScheduleFileWatcher(watcher, stoppingToken);
                }
            }
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
