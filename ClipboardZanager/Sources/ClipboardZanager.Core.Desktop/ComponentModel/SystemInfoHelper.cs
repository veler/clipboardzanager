using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Windows.Networking.Connectivity;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.Interop;
using ClipboardZanager.Core.Desktop.Interop.Structs;
using ClipboardZanager.Core.Desktop.Models;
using System.Diagnostics;

namespace ClipboardZanager.Core.Desktop.ComponentModel
{
    /// <summary>
    /// Provides a set of functions used to retrieve information about the operating system.
    /// </summary>
    internal static class SystemInfoHelper
    {
        /// <summary>
        /// Make a small HTTP request to check if internet access work or not.
        /// </summary>
        /// <returns>Returns True if the application have access to internet</returns>
        internal static bool CheckForInternetConnection()
        {
            try
            {
                int desc;
                if (!NativeMethods.InternetGetConnectedState(out desc, 0))
                {
                    return false;
                }

                using (var client = new WebClient())
                using (client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Try to retrieve the title tag of an HTML page from a URI.
        /// </summary>
        /// <param name="uri">The uri of the page</param>
        /// <returns>Returns the title of the page</returns>
        internal static string GetWebPageTitle(string uri)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    var html = client.DownloadString(uri);
                    var title = Regex.Match(html, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                    return title;
                }
            }
            catch { }

            return string.Empty;
        }

        /// <summary>
        /// Determines whether the current internet connection is a metered connection.
        /// </summary>
        /// <returns>Returns True if the connection is metered.</returns>
        internal static bool IsMeteredConnection()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (connectionProfile == null)
            {
                // not connected to any network.
                return false;
            }

            var connectionCost = NetworkInformation.GetInternetConnectionProfile().GetConnectionCost();
            return connectionCost.NetworkCostType != NetworkCostType.Unknown && connectionCost.NetworkCostType != NetworkCostType.Unrestricted;
        }

        /// <summary>
        /// Determines whether a screen reader is running on the system.
        /// </summary>
        /// <returns>Returns True if a screen reader is running.</returns>
        internal static bool IsScreenReaderRunning()
        {
            if (Debugger.IsAttached)
            {
                return true;
            }

            var result = false;
            NativeMethods.SystemParametersInfo(Consts.SPI_GETSCREENREADER, 0, ref result, 0);
            return result;
        }

        /// <summary>
        /// Retrieve the .exe file path from a process id
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <returns>The full path to the .exe file</returns>
        internal static string GetExecutablePath(int processId)
        {
            if (CoreHelper.IsUnitTesting())
            {
                return @"C:\Windows\explorer.exe";
            }

            var buffer = new StringBuilder(1024);
            var hprocess = NativeMethods.OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, processId);

            if (hprocess != IntPtr.Zero)
            {
                try
                {
                    var size = buffer.Capacity;
                    if (NativeMethods.QueryFullProcessImageName(hprocess, 0, buffer, out size))
                    {
                        return buffer.ToString();
                    }
                }
                finally
                {
                    NativeMethods.CloseHandle(hprocess);
                }
            }

            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Retrieves information about all monitors.
        /// </summary>
        /// <returns>An array of <see cref="ScreenInfo"/></returns>
        internal static ScreenInfo[] GetAllScreenInfos()
        {
            var result = new List<ScreenInfo>();
            var allScreens = Screen.AllScreens.ToList();

            var primaryScreenIndex = allScreens.FindIndex(s => s.Primary);
            var primaryScreen = allScreens[primaryScreenIndex];
            var screenScale = GetMonitorScaleFactor(primaryScreen);
            var primaryScreenScaleFactor = screenScale / 100.0;

            result.Add(new ScreenInfo
            {
                Index = primaryScreenIndex,
                DeviceName = primaryScreen.DeviceName,
                Scale = screenScale,
                Primary = true,
                Bounds = new Rect(primaryScreen.Bounds.Left, primaryScreen.Bounds.Top, primaryScreen.Bounds.Width, primaryScreen.Bounds.Height),
                OriginalBounds = new Rect(primaryScreen.Bounds.Left, primaryScreen.Bounds.Top, primaryScreen.Bounds.Width, primaryScreen.Bounds.Height)
            });

            for (var i = 0; i < allScreens.Count; i++)
            {
                if (i == primaryScreenIndex)
                {
                    continue;
                }

                var screen = allScreens[i];

                screenScale = GetMonitorScaleFactor(screen);

                double left = screen.Bounds.Left;
                if (screen.Bounds.Left != 0 && primaryScreenScaleFactor > screenScale / 100.0)
                {
                    left = screen.Bounds.Left / primaryScreenScaleFactor * screenScale / 100;
                }

                double top = screen.Bounds.Top;
                if (screen.Bounds.Top != 0 && primaryScreenScaleFactor > screenScale / 100.0)
                {
                    top = screen.Bounds.Top / primaryScreenScaleFactor * screenScale / 100;
                }

                var width = screen.Bounds.Width / primaryScreenScaleFactor * screenScale / 100;

                var height = screen.Bounds.Height / primaryScreenScaleFactor * screenScale / 100;

                result.Add(new ScreenInfo
                {
                    Index = i,
                    DeviceName = screen.DeviceName,
                    Scale = screenScale,
                    Bounds = new Rect((int)left, (int)top, (int)width, (int)height),
                    OriginalBounds = new Rect(screen.Bounds.Left, screen.Bounds.Top, screen.Bounds.Width, screen.Bounds.Height),
                    Primary = false
                });
            }

            return result.ToArray();
        }

        /// <summary>
        /// Retrieve the scale factor of the specified screen
        /// </summary>
        /// <param name="screen">The screen</param>
        /// <returns>Return a number between 100 and 300. The value is in percent.</returns>
        private static int GetMonitorScaleFactor(Screen screen)
        {
            var point = new System.Drawing.Point(screen.Bounds.Left + 1, screen.Bounds.Top + 1);
            var hMonitor = NativeMethods.MonitorFromPoint(point, Consts.MONITOR_DEFAULTTONEAREST);
            int screenScale;
            NativeMethods.GetScaleFactorForMonitor(hMonitor, out screenScale);
            return screenScale;
        }
    }
}
