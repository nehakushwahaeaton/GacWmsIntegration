using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Services;
using GacWmsIntegration.FileProcessor.Models;
using GacWmsIntegration.FileProcessor.Services;
using GacWmsIntegration.Infrastructure.Data;
using GacWmsIntegration.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GacWmsIntegration.FileProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            try
            {
                Log.Information("Starting GAC WMS File Processor");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "GAC WMS File Processor terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    // Add configuration
                    services.Configure<FileProcessingConfig>(
                        hostContext.Configuration.GetSection("FileProcessing"));

                    // Add DbContext
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(
                            hostContext.Configuration.GetConnectionString("DefaultConnection")));

                    // Add Core Services
                    services.AddScoped<ICustomerService, CustomerService>();
                    services.AddScoped<IProductService, ProductService>();
                    services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
                    services.AddScoped<ISalesOrderService, SalesOrderService>();

                    // Add Infrastructure Services
                    services.AddScoped<IWmsService, WmsService>();
                    services.AddScoped<IWmsApiClient, WmsApiClient>();

                    // Add File Processor Services
                    services.AddScoped<FileProcessingService>();
                    services.AddHostedService<SchedulerService>();
                });
    }
}
