using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GacWmsIntegration.Infrastructure.Data
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IApplicationDbContext _dbContext;

        public DatabaseHealthCheck(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Try to connect to the database
                var canConnect = await ((DbContext)_dbContext).Database.CanConnectAsync(cancellationToken);

                if (canConnect)
                {
                    return HealthCheckResult.Healthy("Database connection is healthy");
                }
                else
                {
                    return HealthCheckResult.Degraded("Database connection test failed, but API is still functional");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Degraded($"Database connection error: {ex.Message}", ex);
            }
        }
    }
}
