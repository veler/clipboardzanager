using System;
using Newtonsoft.Json;

namespace ClipboardZanager.Core.Desktop.Models
{
    /// <summary>
    /// Represents the information used to identify a part of a clipboard data.
    /// </summary>
    [Serializable]
    internal sealed class DataIdentifier
    {
        /// <summary>
        /// Gets or sets the data file identifier
        /// </summary>
        [JsonProperty]
        internal Guid Identifier { get; set; }

        /// <summary>
        /// Gets or sets the name of the clipboard data format
        /// </summary>
        [JsonProperty]
        internal string FormatName { get; set; }
    }
}
