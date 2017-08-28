using System.Windows.Controls;

namespace ClipboardZanager.Core.Desktop.Models
{
    /// <summary>
    /// Represents a <see cref="ICloudStorageProvider"/> in the UI.
    /// </summary>
    internal sealed class CloudStorageProvider
    {
        #region Properties

        /// <summary>
        /// Gets the name of the cloud service
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a <see cref="ControlTemplate"/> that represents the icon of the cloud service
        /// </summary>
        public ControlTemplate Icon { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize the instance of the <see cref="CloudStorageProvider"/> class
        /// </summary>
        /// <param name="name">the name of the cloud service.</param>
        /// <param name="icon">represents the icon of the cloud service.</param>
        internal CloudStorageProvider(string name, ControlTemplate icon)
        {
            Name = name;
            Icon = icon;
        }

        #endregion
    }
}
