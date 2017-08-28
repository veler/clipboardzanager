using System;
using ClipboardZanager.Core.Desktop.Enums;
using Newtonsoft.Json;

namespace ClipboardZanager.Core.Desktop.Models
{
    /// <summary>
    /// Represents the information about a data from the data entry file on the cloud.
    /// </summary>
    [Serializable]
    internal sealed class CloudDataEntry : DataEntryBase
    {
        /// <summary>
        /// Gets or sets the data to display as a thumbnail.
        /// </summary>
        [JsonProperty]
        internal string ThumbnailValue { get; set; }

        /// <summary>
        /// Gets or sets the type of the value
        /// </summary>
        [JsonProperty]
        internal ThumbnailDataType ThumbnailDataType { get; set; }

        /// <summary>
        /// Gets or sets the icon linked to the data.
        /// </summary>
        [JsonProperty]
        internal string Icon { get; set; }
    }
}
