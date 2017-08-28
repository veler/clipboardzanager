using Newtonsoft.Json;
using System.Windows.Media.Imaging;
using ClipboardZanager.Core.Desktop.ComponentModel;

namespace ClipboardZanager.Core.Desktop.Models
{
    /// <summary>
    /// Represents an application which must be ignored when we copy a data from it.
    /// </summary>
    internal sealed class IgnoredApplication
    {
        [JsonIgnore]
        private BitmapImage _iconBitmap = null;

        /// <summary>
        /// Get or sets the display name of the application. It can be the executable file name or a part of the package name (for a Windows Store app)
        /// </summary>
        [JsonProperty]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets a string that identify the application. It can be the full path to the executable of the fulle package name (for a Windows Store app)
        /// </summary>
        [JsonProperty]
        internal string ApplicationIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the icon of the application
        /// </summary>
        [JsonProperty]
        internal string IconString { get; set; }

        /// <summary>
        /// Gets or sets the icon of the application
        /// </summary>
        [JsonIgnore]
        internal BitmapImage Icon
        {
            get
            {
                if (string.IsNullOrWhiteSpace(IconString))
                {
                    return null;
                }

                return (BitmapImage)DataHelper.ByteArrayToBitmapSource(DataHelper.ByteArrayFromBase64(IconString));
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                IconString = DataHelper.ToBase64(DataHelper.BitmapSourceToByteArray(value));
                _iconBitmap = value;
            }
        }

        /// <summary>
        /// Gets the icon to display in the software.
        /// </summary>
        [JsonIgnore]
        public BitmapImage DisplayedIcon
        {
            get
            {
                // By binding this property to the UI, we are sure that the image is loaded one time for all the screens (and so all the windows that will display this picture).
                if (_iconBitmap == null)
                {
                    _iconBitmap = Icon;
                }

                return _iconBitmap;
            }
        }
    }
}
