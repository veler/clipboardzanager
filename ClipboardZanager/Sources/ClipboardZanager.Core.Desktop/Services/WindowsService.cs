using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Interop;
using ClipboardZanager.Core.Desktop.Interop.Classes;
using ClipboardZanager.Core.Desktop.Interop.Interfaces;
using ClipboardZanager.Core.Desktop.Interop.Structs;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Shared.Services;
using Windows.Management.Deployment;
using Windows.Storage.Streams;

namespace ClipboardZanager.Core.Desktop.Services
{
    /// <summary>
    /// Provides a set of functions designed to retrieve the current accessible windows.
    /// </summary>
    internal sealed class WindowsService : IService
    {
        #region Fields

        private readonly List<WindowsStoreApp> _windowsStoreApps = new List<WindowsStoreApp>();
        private readonly PropertyKey _appUserModelIdKey = new PropertyKey("{" + Consts.AppUserModelIdKey + "}", 5);
        private readonly List<IntPtr> _windowHandlesToIgnore = new List<IntPtr>();
        private bool _refreshDesktopWindowOnly;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the list of <see cref="Window"/>.
        /// </summary>
        internal List<Window> WindowsList { get; private set; }

        /// <summary>
        /// Gets the <see cref="Window"/> that corresponds to the desktop background.
        /// </summary>
        internal Window DesktopWindow { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Raised when the list of windows change.
        /// </summary>
        internal event EventHandler<EventArgs> WindowsListChanged;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void Initialize(IServiceSettingProvider settingProvider)
        {
            WindowsList = new List<Window>();

            using (Logger.Instance.Stopwatch($"{GetType().Name} is retreiving Windows Store apps information."))
            {
                try
                {
                    // We retrieve the installed Windows Store apps.
                    var packageManager = new PackageManager();
                    var packages = packageManager.FindPackagesForUser("");

                    foreach (var package in packages)
                    {
                        var app = new WindowsStoreApp(package);
                        app.InitializeAsync().Wait();
                        _windowsStoreApps.Add(app);
                    }
                }
                catch
                {
                    Logger.Instance.Information($"{GetType().Name} has failed to retrieve Windows Store apps information.");
                }

                Logger.Instance.Information($"{GetType().Name} has found {_windowsStoreApps.Count} Windows Store apps.");
            }

            Logger.Instance.Information($"{GetType().Name} initialized.");
        }

        /// <inheritdoc/>
        public void Reset()
        {
            DesktopWindow = null;
            WindowsList.Clear();
            _windowsStoreApps.Clear();
            _windowHandlesToIgnore.Clear();
            _refreshDesktopWindowOnly = false;
        }

        /// <summary>
        /// Refresh the list of <see cref="Window"/>
        /// </summary>
        /// <param name="refreshDesktopWindowOnly">Defines whether we must only refresh the desktop window information.</param>
        internal void RefreshWindows(bool refreshDesktopWindowOnly = false)
        {
            if (!refreshDesktopWindowOnly)
            {
                WindowsList.Clear();
                WindowsListChanged?.Invoke(this, new EventArgs());
            }

            _refreshDesktopWindowOnly = refreshDesktopWindowOnly;
            Requires.VerifySucceeded(NativeMethods.EnumWindows(EnumWindowsCallback, 0));
        }

        /// <summary>
        /// Add a specific <see cref="System.Windows.Window"/> to the list of ignored windows.
        /// </summary>
        /// <param name="window">The window to ignore</param>
        internal void AddWindowToIgnoreList(System.Windows.Window window)
        {
            Requires.NotNull(window, nameof(window));

            var ignoredWindowHandle = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            _windowHandlesToIgnore.Add(ignoredWindowHandle);
        }

        /// <summary>
        /// Remove the specified <see cref="System.Windows.Window"/> from the list of ignored windows.
        /// </summary>
        /// <param name="window">The window to remove</param>
        internal void RemoveWindowToIgnoreList(System.Windows.Window window)
        {
            Requires.NotNull(window, nameof(window));

            var ignoredWindowHandle = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            _windowHandlesToIgnore.RemoveAll(w => w == ignoredWindowHandle);
        }

        /// <summary>
        /// Retrieves some information about the foreground window, like the title, the icon, and return it.
        /// </summary>
        /// <returns>A <see cref="Window"/> that represents the information about the foreground window</returns>
        internal Window GetForegroundWindow()
        {
            var foregroundWindowHandle = NativeMethods.GetForegroundWindow();
            return GetWindowInformation(foregroundWindowHandle);
        }

        /// <summary>
        /// Callback used everytime a window is detected when we call EnumWindows
        /// </summary>
        /// <param name="windowHandle">The handle of the window</param>
        /// <param name="lParam">The application-defined value given in EnumWindows or EnumDesktopWindows.</param>
        /// <returns>True to continue to search for windows</returns>
        private bool EnumWindowsCallback(IntPtr windowHandle, int lParam)
        {
            // Check if the window is visible, has a border or not, has a title, is the main window of the app (so we ignore the child windows and dialog box)
            var windowStyle = NativeMethods.GetWindowLongA(windowHandle, Consts.GWL_STYLE);
            var isApplicationWindow = (windowStyle & Consts.TARGETWINDOW) == Consts.TARGETWINDOW;
            var isDesktopWindow = windowHandle == NativeMethods.GetShellWindow();
            var isVisible = NativeMethods.IsWindowVisible(windowHandle) && NativeMethods.IsWindow(windowHandle);
            var isChainVisible = IsWindowChainVisible(windowHandle);
            var isInIgnoreList = _windowHandlesToIgnore.Contains(windowHandle);

            if (!isVisible || !isChainVisible || isInIgnoreList || (!isApplicationWindow && !isDesktopWindow) || (_refreshDesktopWindowOnly && !isDesktopWindow))
            {
                return true; //continue enumeration
            }

            if (isDesktopWindow)
            {
                DesktopWindow = new Window(windowHandle, string.Empty, null, string.Empty, null, false);
                return true; //continue enumeration
            }

            var stringBuilder = new StringBuilder(256);
            NativeMethods.GetWindowText(windowHandle, stringBuilder, stringBuilder.Capacity);

            if (string.IsNullOrEmpty(stringBuilder.ToString()))
            {
                return true; //continue enumeration
            }

            var window = GetWindowInformation(windowHandle);
            if (window != null)
            {
                WindowsList.Add(window);
                WindowsListChanged?.Invoke(this, new EventArgs());
            }

            return true; //continue enumeration
        }

        /// <summary>
        /// Retrieves some information about a window, like the title, the icon, and return it.
        /// </summary>
        /// <param name="windowHandle">The handle of the window</param>
        /// <returns>A <see cref="Window"/> that represents the information about the window</returns>
        private Window GetWindowInformation(IntPtr windowHandle)
        {
            var stringBuilder = new StringBuilder(256);
            NativeMethods.GetWindowText(windowHandle, stringBuilder, stringBuilder.Capacity);

            // We retrieve the process id
            int processId;
            NativeMethods.GetWindowThreadProcessId(windowHandle, out processId);

            BitmapImage icon = null;
            var process = Process.GetProcessById(processId);
            var isWindowsStoreApp = false;
            var applicationIdentifier = string.Empty;

            Requires.NotNull(process, nameof(process));

            // If the process corresponds to a Windows Store app (process ApplicationFrameHost
            if (string.Equals(process.ProcessName, Consts.WindowsStoreProcessName, StringComparison.OrdinalIgnoreCase))
            {
                // We retrieve the Win Store App package linked to this window.
                isWindowsStoreApp = true;

                IPropertyStore propStore;
                var iidIPropertyStore = new Guid("{" + Consts.PropertyStore + "}");
                Requires.VerifySucceeded(NativeMethods.SHGetPropertyStoreForWindow(windowHandle, ref iidIPropertyStore, out propStore));

                using (var prop = new PropVariant())
                {
                    Requires.VerifySucceeded(propStore.GetValue(_appUserModelIdKey, prop));

                    var familyName = prop.Value.Split('!').First();
                    var package = _windowsStoreApps.FirstOrDefault(app => string.Equals(app.FamilyName, familyName, StringComparison.Ordinal));
                    if (package != null)
                    {
                        applicationIdentifier = package.FamilyName;
                        // Avoid some Thread problem with the hooking. TODO : Investigate for a better solution
                        var thread = new Thread(() =>
                        {
                            // Then we retrieve the application's icon.
                            var iconTask = GetWindowsStoreIconAsync(package);
                            iconTask.Wait();
                            icon = iconTask.Result;
                        });
                        thread.Start();
                        thread.Join();
                    }
                }
            }
            else
            {
                applicationIdentifier = SystemInfoHelper.GetExecutablePath(process.Id);
                icon = GetWin32WindowIcon(windowHandle, applicationIdentifier);
            }

            if (string.IsNullOrEmpty(applicationIdentifier))
            {
                return null;
            }

            if (icon == null)
            {
                icon = new BitmapImage();
                var test = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\explorer.exe";
                if (applicationIdentifier.ToLower() == (Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\explorer.exe").ToLower() && stringBuilder.ToString() == "") // Desktop
                {
                    Bitmap bitIcon = Icon.ExtractAssociatedIcon(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\explorer.exe").ToBitmap();
                    using (var memory = new MemoryStream())
                    {
                        bitIcon.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                        memory.Position = 0;
                        icon.BeginInit();
                        icon.StreamSource = memory;
                        icon.CacheOption = BitmapCacheOption.OnLoad;
                        icon.EndInit();
                        icon.Freeze();
                    }
                }
                else // Other app without icon
                {
                    icon.BeginInit();
                    icon.UriSource = new Uri("pack://application:,,,/ClipboardZanager;component/Assets/NoIcon.png", UriKind.RelativeOrAbsolute);
                    icon.CacheOption = BitmapCacheOption.OnLoad;
                    icon.EndInit();
                    icon.Freeze();
                }

            }

            return new Window(windowHandle, stringBuilder.ToString(), process, applicationIdentifier, icon, isWindowsStoreApp);
        }

        /// <summary>
        /// Retrieve the icon of a Windows Store app
        /// </summary>
        /// <param name="package">The Windows Store package</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<BitmapImage> GetWindowsStoreIconAsync(WindowsStoreApp package)
        {
            Requires.NotNull(package, nameof(package));

            // if the AppDetails has not been loaded yet and that the icon is requested, to don't make the use wait, we just return null.
            if (package.AppDetails == null)
            {
                return null;
            }

            var appIcon = package.AppDetails.DisplayInfo.GetLogo(new Windows.Foundation.Size(Consts.WindowsIconsSize * 2, Consts.WindowsIconsSize * 2));
            var stream = await appIcon.OpenReadAsync().AsTask().ConfigureAwait(false);
            using (var reader = new DataReader(stream.GetInputStreamAt(0)))
            {
                await reader.LoadAsync((uint)stream.Size);
                var bytes = new byte[stream.Size];
                reader.ReadBytes(bytes);
                using (var ms = new MemoryStream(bytes))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.DecodePixelHeight = Consts.WindowsIconsSize * 2;
                    image.StreamSource = ms;
                    image.EndInit();
                    image.Freeze();
                    return image;
                }
            }
        }

        /// <summary>
        /// Retrieve the icon of a Win32 window (not application).
        /// </summary>
        /// <param name="windowHandle">The handle of the window to retrieve</param>
        /// <param name="processFilePath">The full path to the running process</param>
        /// <returns>The icon</returns>
        private BitmapImage GetWin32WindowIcon(IntPtr windowHandle, string processFilePath)
        {
            var iconHandle = (IntPtr)NativeMethods.SendMessage(windowHandle, Consts.WM_GETICON, true, 0);

            if (iconHandle == IntPtr.Zero)
            {
                iconHandle = GetClassLongPtr(windowHandle, -34);
            }

            if (!(iconHandle == IntPtr.Zero))
            {
                var icon = Icon.FromHandle(iconHandle).ToBitmap();

                if (icon.Height < Consts.WindowsIconsSize && !string.IsNullOrEmpty(processFilePath) && File.Exists(processFilePath))
                {
                    // If the icon is smaller than the desired icon size, we try to get the icon of the .exe which is probably bigger (i.e : the window icon of Notepad is 16x16, and the .exe icon is 64x64, which is good).
                    icon = Icon.ExtractAssociatedIcon(processFilePath)?.ToBitmap();
                }

                if (icon != null)
                {
                    return DataHelper.BitmapToBitmapImage(new Bitmap(icon, Consts.WindowsIconsSize, Consts.WindowsIconsSize), Consts.WindowsIconsSize);
                }
            }

            return null;
        }

        /// <summary>
        /// Detect if the specified window is a child window or not.
        /// </summary>
        /// <param name="windowHandle">The handle of the window</param>
        /// <returns>True if the window is a child of another</returns>
        private bool IsWindowChainVisible(IntPtr windowHandle)
        {
            // Start at the root owner
            var hwndWalk = NativeMethods.GetAncestor(windowHandle);

            // Basically we try get from the parent back to that window
            IntPtr hwndTry;
            while ((hwndTry = NativeMethods.GetLastActivePopup(hwndWalk)) != hwndTry)
            {
                if (NativeMethods.IsWindowVisible(hwndTry))
                {
                    break;
                }

                hwndWalk = hwndTry;
            }
            return hwndWalk == windowHandle;
        }

        /// <summary>
        /// Retrieves the specified value from the WNDCLASSEX structure associated with the specified window.
        /// </summary>
        /// <param name="windowHandle">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The value to be retrieved. To retrieve a value from the extra class memory, specify the positive, zero-based byte offset of the value to be retrieved. Valid values are in the range zero through the number of bytes of extra class memory, minus eight; for example, if you specified 24 or more bytes of extra class memory, a value of 16 would be an index to the third integer.</param>
        /// <returns>If the function succeeds, the return value is the requested value.</returns>
        private IntPtr GetClassLongPtr(IntPtr windowHandle, int nIndex)
        {
            if (IntPtr.Size > 4)
            {
                return NativeMethods.GetClassLongPtr64(windowHandle, nIndex);
            }

            return new IntPtr(NativeMethods.GetClassLongPtr32(windowHandle, nIndex));
        }

        #endregion
    }
}
