using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GacWmsIntegration.Infrastructure.Repositories
{
    public class SyncRepository : ISyncRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<SyncRepository> _logger;

        public SyncRepository(ApplicationDbContext dbContext, ILogger<SyncRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task RecordSyncResultAsync(SyncResult syncResult)
        {
            if (syncResult == null)
            {
                throw new ArgumentNullException(nameof(syncResult));
            }

            try
            {
                // Create a new SyncResultEntity from the SyncResult
                var syncResultEntity = new SyncResultEntity
                {
                    EntityType = syncResult.EntityType,
                    EntityId = syncResult.EntityId,
                    Success = syncResult.Success,
                    ErrorMessage = syncResult.ErrorMessage,
                    SyncDate = syncResult.SyncDate
                };

                // Add the new sync result to the database
                _dbContext.SyncResults.Add(syncResultEntity);
                await _dbContext.SaveChangesAsync();

                // Update the sync status for this entity
                var syncStatus = await _dbContext.SyncStatuses
                    .FirstOrDefaultAsync(s => s.EntityType == syncResult.EntityType && s.EntityId == syncResult.EntityId);

                if (syncStatus == null)
                {
                    // Create a new sync status if it doesn't exist
                    syncStatus = new SyncStatusEntity
                    {
                        EntityType = syncResult.EntityType,
                        EntityId = syncResult.EntityId,
                        Status = syncResult.Success ? "Synced" : "Failed",
                        LastSyncDate = syncResult.SyncDate,
                        RetryCount = 0,
                        LastErrorMessage = syncResult.ErrorMessage
                    };
                    _dbContext.SyncStatuses.Add(syncStatus);
                }
                else
                {
                    // Update the existing sync status
                    syncStatus.Status = syncResult.Success ? "Synced" : "Failed";
                    syncStatus.LastSyncDate = syncResult.SyncDate;
                    syncStatus.LastErrorMessage = syncResult.ErrorMessage;

                    // Increment retry count if this was a failure
                    if (!syncResult.Success)
                    {
                        syncStatus.RetryCount++;
                    }
                    else
                    {
                        // Reset retry count on success
                        syncStatus.RetryCount = 0;
                    }
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording sync result for {EntityType} {EntityId}",
                    syncResult.EntityType, syncResult.EntityId);
                throw;
            }
        }

        public async Task<SyncStatus> GetSyncStatusAsync(string entityType, string entityId)
        {
            if (string.IsNullOrEmpty(entityType))
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            try
            {
                var syncStatusEntity = await _dbContext.SyncStatuses
                    .FirstOrDefaultAsync(s => s.EntityType == entityType && s.EntityId == entityId);

                if (syncStatusEntity == null)
                {
                    return null;
                }

                return new SyncStatus
                {
                    EntityType = syncStatusEntity.EntityType,
                    EntityId = syncStatusEntity.EntityId,
                    Status = syncStatusEntity.Status,
                    LastSyncDate = syncStatusEntity.LastSyncDate,
                    RetryCount = syncStatusEntity.RetryCount,
                    LastErrorMessage = syncStatusEntity.LastErrorMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sync status for {EntityType} {EntityId}", entityType, entityId);
                throw;
            }
        }

        public async Task<IEnumerable<SyncStatus>> GetFailedSynchronizationsAsync()
        {
            try
            {
                var failedSyncEntities = await _dbContext.SyncStatuses
                    .Where(s => s.Status == "Failed")
                    .OrderBy(s => s.LastSyncDate)
                    .ToListAsync();

                return failedSyncEntities.Select(entity => new SyncStatus
                {
                    EntityType = entity.EntityType,
                    EntityId = entity.EntityId,
                    Status = entity.Status,
                    LastSyncDate = entity.LastSyncDate,
                    RetryCount = entity.RetryCount,
                    LastErrorMessage = entity.LastErrorMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting failed synchronizations");
                throw;
            }
        }

        public async Task<IEnumerable<SyncResult>> GetSyncHistoryAsync(string entityType, string entityId)
        {
            if (string.IsNullOrEmpty(entityType))
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            try
            {
                var syncResultEntities = await _dbContext.SyncResults
                    .Where(r => r.EntityType == entityType && r.EntityId == entityId)
                    .OrderByDescending(r => r.SyncDate)
                    .ToListAsync();

                return syncResultEntities.Select(entity => new SyncResult
                {
                    EntityType = entity.EntityType,
                    EntityId = entity.EntityId,
                    Success = entity.Success,
                    ErrorMessage = entity.ErrorMessage,
                    SyncDate = entity.SyncDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sync history for {EntityType} {EntityId}", entityType, entityId);
                throw;
            }
        }

        public async Task<IEnumerable<SyncResult>> GetRecentSyncResultsAsync(int count = 100)
        {
            try
            {
                var syncResultEntities = await _dbContext.SyncResults
                    .OrderByDescending(r => r.SyncDate)
                    .Take(count)
                    .ToListAsync();

                return syncResultEntities.Select(entity => new SyncResult
                {
                    EntityType = entity.EntityType,
                    EntityId = entity.EntityId,
                    Success = entity.Success,
                    ErrorMessage = entity.ErrorMessage,
                    SyncDate = entity.SyncDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent sync results");
                throw;
            }
        }

        public async Task<SyncStatistics> GetSyncStatisticsAsync()
        {
            try
            {
                var statistics = new SyncStatistics
                {
                    TotalSyncCount = await _dbContext.SyncResults.CountAsync(),
                    SuccessfulSyncCount = await _dbContext.SyncResults.CountAsync(r => r.Success),
                    FailedSyncCount = await _dbContext.SyncResults.CountAsync(r => !r.Success),
                    PendingSyncCount = await _dbContext.SyncStatuses.CountAsync(s => s.Status == "Pending"),
                    LastSyncDate = await _dbContext.SyncResults.MaxAsync(r => (DateTime?)r.SyncDate) ?? DateTime.MinValue
                };

                // Get counts by entity type
                var entityTypeCounts = await _dbContext.SyncResults
                    .GroupBy(r => r.EntityType)
                    .Select(g => new { EntityType = g.Key, Count = g.Count() })
                    .ToListAsync();

                foreach (var item in entityTypeCounts)
                {
                    statistics.SyncCountByEntityType[item.EntityType] = item.Count;
                }

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sync statistics");
                throw;
            }
        }

        public async Task ClearSyncHistoryAsync(DateTime olderThan)
        {
            try
            {
                // Delete old sync results but keep the most recent result for each entity
                // to maintain the sync status
                var oldSyncResults = await _dbContext.SyncResults
                    .Where(r => r.SyncDate < olderThan)
                    .ToListAsync();

                // Get the most recent sync result for each entity
                var mostRecentResults = await _dbContext.SyncResults
                    .GroupBy(r => new { r.EntityType, r.EntityId })
                    .Select(g => g.OrderByDescending(r => r.SyncDate).First())
                    .ToListAsync();

                // Remove the most recent results from the list of results to delete
                var resultsToDelete = oldSyncResults
                    .Where(r => !mostRecentResults.Any(mr =>
                        mr.EntityType == r.EntityType &&
                        mr.EntityId == r.EntityId &&
                        mr.SyncDate == r.SyncDate))
                    .ToList();

                _dbContext.SyncResults.RemoveRange(resultsToDelete);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Cleared {Count} sync history records older than {Date}",
                    resultsToDelete.Count, olderThan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing sync history older than {Date}", olderThan);
                throw;
            }
        }
    }

    // Entity classes for database storage
    public class SyncResultEntity
    {
        public int Id { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime SyncDate { get; set; }
    }

    public class SyncStatusEntity
    {
        public int Id { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string Status { get; set; } // "Pending", "Synced", "Failed"
        public DateTime? LastSyncDate { get; set; }
        public int RetryCount { get; set; }
        public string LastErrorMessage { get; set; }
    }
}
