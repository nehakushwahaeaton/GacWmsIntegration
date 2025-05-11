using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Services;
using GacWmsIntegration.FileProcessor.Interfaces;
using GacWmsIntegration.FileProcessor.Models;
using GacWmsIntegration.FileProcessor.Services;
using GacWmsIntegration.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

try
{
    // Check if appsettings.json exists
    string appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
    if (!File.Exists(appSettingsPath))
    {
        Console.WriteLine($"WARNING: Configuration file not found at: {appSettingsPath}");
        // Create a minimal appsettings.json file
        File.WriteAllText(appSettingsPath, @"{
  ""Logging"": {
    ""LogLevel"": {
      ""Default"": ""Information"",
      ""Microsoft"": ""Warning""
    }
  },
  ""FileProcessing"": {
    ""FileWatchers"": []
  }
}");
        Console.WriteLine("Created a minimal configuration file.");
    }

    var host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostContext, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
            config.AddEnvironmentVariables();
        })
        .ConfigureServices((hostContext, services) =>
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(hostContext.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            services.AddLogging(builder => builder.AddSerilog(dispose: true));

            // Register infrastructure services
            //services.AddInfrastructureServices(hostContext.Configuration);

            // Register DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    hostContext.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Register ApplicationDbContext as IApplicationDbContext
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            // Add Core Services
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddScoped<ISalesOrderService, SalesOrderService>();

            // Register file processor services
            services.Configure<FileProcessingConfig>(hostContext.Configuration.GetSection("FileProcessing"));
            services.AddSingleton<IXmlParserService, XmlParserService>(); // Register IXmlParserService
            services.AddSingleton<FileProcessingService>();
            services.AddHostedService<SchedulerService>();
        })
        .UseSerilog()
        .Build();

    Log.Information("Starting GAC WMS File Processor");
    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    Log.Fatal(ex, "GAC WMS File Processor terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
