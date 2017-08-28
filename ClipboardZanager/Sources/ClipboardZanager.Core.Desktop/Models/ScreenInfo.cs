using ClipboardZanager.Core.Desktop.Interop.Structs;

namespace ClipboardZanager.Core.Desktop.Models
{
    /// <summary>
    /// Represents information about a monitor
    /// </summary>
    internal sealed class ScreenInfo
    {
        /// <summary>
        /// Gets or sets the index of the monitor
        /// </summary>
        internal int Index { get; set; }

        /// <summary>
        /// Gets or sets wether the monitor is the primary screen.
        /// </summary>
        internal bool Primary { get; set; }

        /// <summary>
        /// Gets or sets the device name
        /// </summary>
        internal string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the location and the size of the monitor after removing the scale
        /// </summary>
        internal Rect Bounds { get; set; }

        /// <summary>
        /// Gets or sets the location and the size of the monitor
        /// </summary>
        internal Rect OriginalBounds { get; set; }

        /// <summary>
        /// Gets or sets the scale of the UI. 125 means 125%.
        /// </summary>
        internal int Scale { get; set; }
    }
}
