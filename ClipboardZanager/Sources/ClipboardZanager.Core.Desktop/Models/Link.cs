using System;

namespace ClipboardZanager.Core.Desktop.Models
{
    /// <summary>
    /// Represents a link in a <see cref="Thumbnail"/>
    /// </summary>
    [Serializable]
    internal sealed class Link
    {
        /// <summary>
        /// Gets or sets the uri of the link.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Gets or sets the title of the page.
        /// </summary>
        public string Title { get; set; }

        public Link()
        {
            Title = string.Empty;
        }
    }
}
