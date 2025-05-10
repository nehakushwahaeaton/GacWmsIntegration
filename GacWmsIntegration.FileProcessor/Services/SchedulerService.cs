using GacWmsIntegration.FileProcessor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GacWmsIntegration.FileProcessor.Services
{
    /// <summary>
    /// Background service that schedules file processing at regular intervals
    /// </summary>
    public class SchedulerService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly ILogger<SchedulerService> _logger;
        private readonly FileProcessingConfig _config;
        private readonly FileProcessingService _fileProcessingService;
        private Timer _timer;

        public SchedulerService(
            IOptions<FileProcessingConfig> config,
            FileProcessingService fileProcessingService,
            ILogger<SchedulerService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
            _fileProcessingService = fileProcessingService ?? throw new ArgumentNullException(nameof(fileProcessingService));
        }

        /// <summary>
        /// Start the background service
        /// </summary>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduler Service is starting");

            // Don't await this as it would block the service from starting
            _timer = new Timer(DoWork, stoppingToken, TimeSpan.Zero,
                TimeSpan.FromSeconds(_config.ProcessingIntervalSeconds));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Execute the file processing work
        /// </summary>
        private async void DoWork(object state)
        {
            var cancellationToken = (CancellationToken)state;

            try
            {
                // Stop the timer while processing to prevent overlapping executions
                _timer?.Change(Timeout.Infinite, 0);

                _logger.LogInformation("Starting scheduled file processing at: {time}", DateTimeOffset.Now);

                //await _fileProcessingService.ProcessFilesAsync(cancellationToken);

                _logger.LogInformation("Completed scheduled file processing at: {time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during scheduled file processing: {ErrorMessage}", ex.Message);
            }
            finally
            {
                // Restart the timer if not cancelled
                if (!cancellationToken.IsCancellationRequested)
                {
                    _timer?.Change(TimeSpan.FromSeconds(_config.ProcessingIntervalSeconds),
                        TimeSpan.FromSeconds(_config.ProcessingIntervalSeconds));
                }
            }
        }

        /// <summary>
        /// Stop the background service
        /// </summary>
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduler Service is stopping");

            _timer?.Change(Timeout.Infinite, 0);

            return base.StopAsync(stoppingToken);
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public override void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }
}


