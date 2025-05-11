using System.Diagnostics.CodeAnalysis;

namespace GacWmsIntegration.FileProcessor.Models
{
    [ExcludeFromCodeCoverage]
    public class FileProcessingConfig
    {
        public List<FileWatcherConfig> FileWatchers { get; set; } = new List<FileWatcherConfig>();
    }
}
