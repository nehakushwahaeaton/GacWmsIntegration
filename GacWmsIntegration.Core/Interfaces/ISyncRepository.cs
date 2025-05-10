using GacWmsIntegration.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GacWmsIntegration.Core.Interfaces
{
    public interface ISyncRepository
    {
        // Record a synchronization result
        Task RecordSyncResultAsync(SyncResult syncResult);

        // Get the synchronization status for a specific entity
        Task<SyncStatus> GetSyncStatusAsync(string entityType, string entityId);

        // Get all failed synchronizations for retry
        Task<IEnumerable<SyncStatus>> GetFailedSynchronizationsAsync();

        // Get synchronization history for a specific entity
        Task<IEnumerable<SyncResult>> GetSyncHistoryAsync(string entityType, string entityId);

        // Get recent synchronization results (for reporting)
        Task<IEnumerable<SyncResult>> GetRecentSyncResultsAsync(int count = 100);

        // Get synchronization statistics
        Task<SyncStatistics> GetSyncStatisticsAsync();

        // Clear old synchronization history
        Task ClearSyncHistoryAsync(DateTime olderThan);
    }

    public class SyncStatistics
    {
        public int TotalSyncCount { get; set; }
        public int SuccessfulSyncCount { get; set; }
        public int FailedSyncCount { get; set; }
        public int PendingSyncCount { get; set; }
        public DateTime LastSyncDate { get; set; }
        public Dictionary<string, int> SyncCountByEntityType { get; set; } = new Dictionary<string, int>();
    }
}
