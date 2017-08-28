using System;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace ClipboardZanager.Core.Desktop.Models
{
    /// <summary>
    /// Represents a window and its information.
    /// </summary>
    internal sealed class Window
    {
        #region Properties

        /// <summary>
        /// Gets the handle of the window
        /// </summary>
        internal IntPtr Handle { get; }

        /// <summary>
        /// Gets the window's title
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the process associated to the window. If it is a Window Store app, the process will be "ApplicationFrameHost".
        /// </summary>
        internal Process Process { get; }

        /// <summary>
        /// Gets the icon of the application.
        /// </summary>
        public BitmapImage Icon { get; }

        /// <summary>
        /// Gets a value that defines the window is from a Windows Store app.
        /// </summary>
        internal bool IsWindowsStoreApp { get; }

        /// <summary>
        /// Gets a <see cref="string"/> that is used to identify the application linked to this window. It can be the full path to the process or the package name (for a Windows Store app).
        /// </summary>
        internal string ApplicationIdentifier { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="Window"/> class
        /// </summary>
        /// <param name="handle">The handle of the window</param>
        /// <param name="title">The title of the window</param>
        /// <param name="process">The process associated to the window</param>
        /// <param name="applicationIdentifier">Used to identify the application linked to this window. It can be the full path to the process or the package name (for a Windows Store app)</param>
        /// <param name="icon">The icon of the window or the package</param>
        /// <param name="isWindowsStoreApp">Defines whether the window corresponds to a Windows Store app or not</param>
        internal Window(IntPtr handle, string title, Process process, string applicationIdentifier, BitmapImage icon, bool isWindowsStoreApp)
        {
            Handle = handle;
            Title = title;
            Process = process;
            Icon = icon;
            IsWindowsStoreApp = isWindowsStoreApp;
            ApplicationIdentifier = applicationIdentifier;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override string ToString()
        {
            return Title;
        }

        #endregion
    }
}
