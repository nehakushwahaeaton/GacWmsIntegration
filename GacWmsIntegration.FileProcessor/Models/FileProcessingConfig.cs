using System.Diagnostics.CodeAnalysis;

namespace GacWmsIntegration.FileProcessor.Models
{
    [ExcludeFromCodeCoverage]
    public class FileProcessingConfig
    {
        public FileWatcherConfig[] FileWatchers { get; set; } = Array.Empty<FileWatcherConfig>();
        public int ProcessingIntervalMinutes { get; set; } = 5;
    }
}
