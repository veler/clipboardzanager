using System;

namespace ClipboardZanager.Shared.CloudStorage
{
    /// <summary>
    /// Represents a file in the cloud provided by a <see cref="ICloudStorageProvider"/>.
    /// </summary>
    public class CloudFile
    {
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the full path to the file.
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last motification in Utc format on the server.
        /// </summary>
        public DateTime? LastModificationUtcDate { get; set; }
    }
}
