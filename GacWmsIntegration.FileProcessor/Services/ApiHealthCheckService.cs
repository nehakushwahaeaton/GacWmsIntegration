using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GacWmsIntegration.FileProcessor.Services
{
    public class ApiHealthCheckService : IHostedService, IDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ApiHealthCheckService> _logger;
        private readonly IConfiguration _configuration;
        private readonly SchedulerService _schedulerService;
        private Timer? _timer;
        private bool _apiIsHealthy = false;
        private const int MaxRetryAttempts = 5;
        private const int InitialRetryDelaySeconds = 5;
        private int _currentRetryAttempts = 0;

        public ApiHealthCheckService(
            IHttpClientFactory httpClientFactory,
            ILogger<ApiHealthCheckService> logger,
            IConfiguration configuration,
            SchedulerService schedulerService)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
            _schedulerService = schedulerService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("API Health Check Service starting");
            // Create a timer that checks the API health every 5 seconds
            _timer = new Timer(CheckApiHealth, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }

        private async void CheckApiHealth(object? state)
        {
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7299";
            var client = _httpClientFactory.CreateClient();
            int delaySeconds = InitialRetryDelaySeconds;

            try
            {
                var response = await client.GetAsync($"{apiBaseUrl}/health");
                if (response.IsSuccessStatusCode)
                {
                    if (!_apiIsHealthy)
                    {
                        _apiIsHealthy = true;
                        _logger.LogInformation("API is now healthy. File processing can begin.");
                        // Start the scheduler service
                        _schedulerService.StartProcessing();
                    }
                    _currentRetryAttempts = 0; // Reset retry counter on success
                    return;
                }
                else
                {
                    _logger.LogWarning("API health check failed with status code: {StatusCode}", response.StatusCode);
                    _apiIsHealthy = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking API health: {Message}", ex.Message);
                _apiIsHealthy = false;

                _currentRetryAttempts++;
                if (_currentRetryAttempts >= MaxRetryAttempts)
                {
                    _logger.LogError("API health check failed after {MaxRetryAttempts} attempts. Will continue checking but file processing is paused.", MaxRetryAttempts);
                    // Don't stop checking, but log the error
                    _currentRetryAttempts = 0; // Reset to continue checking
                }
                else
                {
                    _logger.LogInformation("Retrying API health check in {DelaySeconds} seconds... (Attempt {CurrentAttempt}/{MaxAttempts})",
                        delaySeconds, _currentRetryAttempts, MaxRetryAttempts);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("API Health Check Service stopping");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
