using System.Windows.Media;

namespace ClipboardZanager.Core.Desktop.Models
{
    /// <summary>
    /// Represents a new that can be displayed in the First Start wizard.
    /// </summary>
    internal sealed class SoftwareNewItem
    {
        /// <summary>
        /// Gets or sets the icon of the new. This icon must be a character from <code>Segoe MDL2 Assets</code>.
        /// </summary>
        public char Icon { get; set; }

        /// <summary>
        /// Gets or sets the title of the new.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a description of the new.
        /// </summary>
        public string Description { get; set; }
    }
}
