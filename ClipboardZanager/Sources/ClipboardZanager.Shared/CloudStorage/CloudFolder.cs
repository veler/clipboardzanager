using System.Collections.Generic;

namespace ClipboardZanager.Shared.CloudStorage
{
    /// <summary>
    /// Represents a folder in the cloud provided by a <see cref="ICloudStorageProvider"/>.
    /// </summary>
    public class CloudFolder
    {
        /// <summary>
        /// Gets or sets the name of the folder.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the size of the folder's content.
        /// </summary>
        public long? Size { get; set; }

        /// <summary>
        /// Gets or sets the full path to the folder.
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Gets or sets the list of files inside the folder.
        /// </summary>
        public List<CloudFile> Files { get; set; }
    }
}
