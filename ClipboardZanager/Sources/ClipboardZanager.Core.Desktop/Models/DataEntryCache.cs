using System;
using ClipboardZanager.Core.Desktop.Enums;

namespace ClipboardZanager.Core.Desktop.Models
{
    /// <summary>
    /// Represents the status of a data entry.
    /// </summary>
    [Serializable]
    internal sealed class DataEntryCache
    {
        /// <summary>
        /// Gets or sets the data entry identifier
        /// </summary>
        internal Guid Identifier { get; set; }

        /// <summary>
        /// Gets or sets the status of the data.
        /// </summary>
        internal DataEntryStatus Status { get; set; }
    }
}
