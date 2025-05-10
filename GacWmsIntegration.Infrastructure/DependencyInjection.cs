using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Infrastructure.Data;
using GacWmsIntegration.Infrastructure.Repositories;
using GacWmsIntegration.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GacWmsIntegration.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information));
            ;

            // Register IApplicationDbContext
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            // Register repositories
            services.AddScoped<ISyncRepository, SyncRepository>();

            // Register services
            services.AddScoped<IWmsService, WmsService>();
            services.AddHttpClient<IWmsApiClient, WmsApiClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["WmsApi:BaseUrl"]);
                client.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("WmsApi:Timeout", 30));
                client.DefaultRequestHeaders.Add("X-API-Key", configuration["WmsApi:ApiKey"]);
            });

            return services;
        }
    }
}
