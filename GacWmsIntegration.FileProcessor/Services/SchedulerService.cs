using Cronos;
using GacWmsIntegration.FileProcessor.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GacWmsIntegration.FileProcessor.Services
{
    public class SchedulerService : BackgroundService
    {
        private readonly ILogger<SchedulerService> _logger;
        private readonly FileProcessingService _fileProcessingService;
        private readonly FileProcessingConfig _config;
        private readonly SemaphoreSlim _startSemaphore = new SemaphoreSlim(0, 1);
        private bool _isProcessingEnabled = false;
        private readonly Dictionary<string, DateTime> _lastRunTimes = new Dictionary<string, DateTime>();
        private readonly Dictionary<string, DateTime> _nextRunTimes = new Dictionary<string, DateTime>();

        public SchedulerService(
            ILogger<SchedulerService> logger,
            FileProcessingService fileProcessingService,
            IOptions<FileProcessingConfig> config)
        {
            _logger = logger;
            _fileProcessingService = fileProcessingService;
            _config = config.Value;

            // Initialize last run times for each watcher with UTC time
            foreach (var watcher in _config.FileWatchers)
            {
                _lastRunTimes[watcher.Name] = DateTime.UtcNow.AddMinutes(-5); // Start 5 minutes ago to ensure first run
                _nextRunTimes[watcher.Name] = DateTime.UtcNow; // Will be calculated properly on first check
            }
        }

        public async Task StartProcessing()
        {
            _logger.LogInformation("Enabling file processing");
            _isProcessingEnabled = true;

            // Release the semaphore if it's not already released
            if (_startSemaphore.CurrentCount == 0)
            {
                _startSemaphore.Release();
                _logger.LogInformation("Semaphore released to start file processing.");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduler Service is waiting for API to be available...");

            // Wait for the API to be available
            await _startSemaphore.WaitAsync(stoppingToken);
            _logger.LogInformation("File processing has been enabled");

            // Release immediately so we can continue processing in future iterations
            _startSemaphore.Release();

            // Calculate initial next run times
            foreach (var watcher in _config.FileWatchers)
            {
                CalculateNextRunTime(watcher);
            }

            // Log initial schedule
            LogSchedule();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_isProcessingEnabled)
                    {
                        bool anyProcessed = false;

                        // Check each file watcher to see if it's time to process
                        foreach (var watcher in _config.FileWatchers)
                        {
                            if (DateTime.UtcNow >= _nextRunTimes[watcher.Name])
                            {
                                _logger.LogInformation("Starting scheduled file processing for watcher: {WatcherName}", watcher.Name);
                                await _fileProcessingService.ProcessFilesAsync(watcher, stoppingToken);
                                _lastRunTimes[watcher.Name] = DateTime.UtcNow; // Update last run time to UTC
                                _logger.LogInformation("Completed scheduled file processing for watcher: {WatcherName}", watcher.Name);

                                // Calculate next run time
                                CalculateNextRunTime(watcher);

                                anyProcessed = true;
                            }
                        }

                        // Log the schedule if any watcher was processed
                        if (anyProcessed)
                        {
                            LogSchedule();
                        }
                    }

                    // Wait a short time before checking schedules again
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Normal shutdown, don't treat as error
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in scheduler service");
                    // Wait a bit before retrying after an error
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }

            _logger.LogInformation("Scheduler Service is shutting down");
        }

        private void CalculateNextRunTime(FileWatcherConfig watcher)
        {
            try
            {
                DateTime nowUtc = DateTime.UtcNow;

                // If no cron schedule is specified, use the global interval
                if (string.IsNullOrWhiteSpace(watcher.CronSchedule))
                {
                    int intervalMinutes = _config.ProcessingIntervalMinutes > 0 ? _config.ProcessingIntervalMinutes : 1;
                    _nextRunTimes[watcher.Name] = _lastRunTimes[watcher.Name].AddMinutes(intervalMinutes);
                    return;
                }

                // Parse the cron expression
                CronExpression expression = CronExpression.Parse(watcher.CronSchedule);

                // Get the next occurrence from now
                DateTime? nextOccurrence = expression.GetNextOccurrence(nowUtc);

                if (nextOccurrence.HasValue)
                {
                    _nextRunTimes[watcher.Name] = nextOccurrence.Value;
                }
                else
                {
                    // Fallback if no next occurrence (shouldn't happen with valid cron expressions)
                    _nextRunTimes[watcher.Name] = nowUtc.AddMinutes(_config.ProcessingIntervalMinutes > 0 ? _config.ProcessingIntervalMinutes : 1);
                    _logger.LogWarning("Could not determine next run time from cron expression for {WatcherName}. Using default interval.", watcher.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating next run time for watcher {WatcherName}: {Schedule}",
                    watcher.Name, watcher.CronSchedule);

                // Default to using the global interval if there's an error with the cron expression
                int intervalMinutes = _config.ProcessingIntervalMinutes > 0 ? _config.ProcessingIntervalMinutes : 1;
                _nextRunTimes[watcher.Name] = DateTime.UtcNow.AddMinutes(intervalMinutes);
            }
        }

        private void LogSchedule()
        {
            _logger.LogInformation("=== File Processing Schedule ===");
            foreach (var watcher in _config.FileWatchers)
            {
                string nextRunLocal = _nextRunTimes[watcher.Name].ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                TimeSpan waitTime = _nextRunTimes[watcher.Name] - DateTime.UtcNow;
                string waitTimeStr = waitTime.TotalMinutes < 1
                    ? $"{waitTime.TotalSeconds:F0} seconds"
                    : $"{waitTime.TotalMinutes:F1} minutes";

                _logger.LogInformation("{WatcherName}: Next run at {NextRunTime} (in {WaitTime})",
                    watcher.Name, nextRunLocal, waitTimeStr);
            }
            _logger.LogInformation("==============================");
        }
    }
}
