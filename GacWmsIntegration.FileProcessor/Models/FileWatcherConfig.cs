using System.Diagnostics.CodeAnalysis;

namespace GacWmsIntegration.FileProcessor.Models
{
    [ExcludeFromCodeCoverage]
    public class FileWatcherConfig
    {
        public string Name { get; set; } = string.Empty;
        public string DirectoryPath { get; set; } = string.Empty;
        public string FilePattern { get; set; } = "*.xml";
        public string CronSchedule { get; set; } = "*/5 * * * *"; // Default: every 5 minutes
        public FileType FileType { get; set; }
        public bool ArchiveProcessedFiles { get; set; } = true;
        public string ArchivePath { get; set; } = string.Empty;
        public int MaxRetryAttempts { get; set; } = 3;
    }
}
