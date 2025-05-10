using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GacWmsIntegration.FileProcessor.Models
{
    /// <summary>
    /// Configuration for file processing operations
    /// </summary>
    public class FileProcessingConfig
    {
        /// <summary>
        /// Base directory for file processing
        /// </summary>
        public string BaseDirectory { get; set; }

        /// <summary>
        /// Directory for input files
        /// </summary>
        public string InputDirectory { get; set; }

        /// <summary>
        /// Directory for processed files
        /// </summary>
        public string ProcessedDirectory { get; set; }

        /// <summary>
        /// Directory for error files
        /// </summary>
        public string ErrorDirectory { get; set; }

        /// <summary>
        /// Directory for backup files
        /// </summary>
        public string BackupDirectory { get; set; }

        /// <summary>
        /// File processing interval in seconds
        /// </summary>
        public int ProcessingIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Maximum number of retries for failed processing
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Delay between retries in seconds
        /// </summary>
        public int RetryDelaySeconds { get; set; } = 300;

        /// <summary>
        /// Whether to delete processed files after processing
        /// </summary>
        public bool DeleteProcessedFiles { get; set; } = false;

        /// <summary>
        /// Whether to move processed files to the processed directory
        /// </summary>
        public bool MoveProcessedFiles { get; set; } = true;

        /// <summary>
        /// Whether to move error files to the error directory
        /// </summary>
        public bool MoveErrorFiles { get; set; } = true;

        /// <summary>
        /// Whether to create a backup of files before processing
        /// </summary>
        public bool CreateBackup { get; set; } = true;

        /// <summary>
        /// File patterns to process
        /// </summary>
        public List<FilePatternConfig> FilePatterns { get; set; } = new List<FilePatternConfig>();
    }

    /// <summary>
    /// Configuration for file patterns to process
    /// </summary>
    public class FilePatternConfig
    {
        /// <summary>
        /// Name of the file pattern
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// File pattern to match (e.g., "*.csv", "order_*.xml")
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// Type of file (e.g., "PurchaseOrder", "SalesOrder", "Product", "Customer")
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// Format of the file (e.g., "CSV", "XML", "JSON", "Excel")
        /// </summary>
        public string FileFormat { get; set; }

        /// <summary>
        /// Whether this pattern is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Custom processing options for this file pattern
        /// </summary>
        public Dictionary<string, string> ProcessingOptions { get; set; } = new Dictionary<string, string>();
    }
}
