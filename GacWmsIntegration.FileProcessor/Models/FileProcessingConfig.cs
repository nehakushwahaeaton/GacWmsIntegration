using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GacWmsIntegration.FileProcessor.Models
{
    public class FileProcessingConfig
    {
        public List<FileWatcherConfig> FileWatchers { get; set; } = new List<FileWatcherConfig>();
    }
}
