using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ClipboardZanager.Core.Desktop.Models
{
    /// <summary>
    /// Represents the basic information of a data entry locally or on a cloud server.
    /// </summary>
    [Serializable]
    internal class DataEntryBase
    {
        /// <summary>
        /// Gets or sets the data entry identifier
        /// </summary>
        [JsonProperty]
        internal Guid Identifier { get; set; }

        /// <summary>
        /// Gets or sets the list of identifiers for a clipboard data
        /// </summary>
        [JsonProperty]
        internal List<DataIdentifier> DataIdentifiers { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> that defines when the data has been copied.
        /// </summary>
        [JsonProperty]
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets a value that defines whether this data is a favorite or not.
        /// </summary>
        [JsonProperty]
        public bool IsFavorite { get; set; }
    }
}
