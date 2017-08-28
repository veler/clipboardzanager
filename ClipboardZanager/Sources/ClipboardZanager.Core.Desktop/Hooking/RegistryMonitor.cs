using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.Interop;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Logs;
using Microsoft.Win32;

namespace ClipboardZanager.Core.Desktop.Hooking
{
    /// <summary>
    /// Provides a monitor on the Windows registry.
    /// </summary>
    internal sealed class RegistryMonitor : IPausable
    {
        #region Fields

        private readonly string _registryPath;
        private readonly RegistryNotifyChange _filter;

        private IntPtr _monitoredKeyHandle;
        private Thread _monitorThread;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current monitored registry key.
        /// </summary>
        internal RegistryKey MonitoredKey { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a change has been detected in the registry key. We don't know which value is.
        /// </summary>
        public event EventHandler Changed;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="RegistryMonitor"/> class
        /// </summary>
        /// <param name="registryPath"></param>
        /// <param name="filter"></param>
        public RegistryMonitor(string registryPath, RegistryNotifyChange filter)
        {
            _registryPath = registryPath.ToUpper();
            _filter = filter;

            Initialize();
            Resume();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void Pause()
        {
            lock (this)
            {
                Changed = null;

                if (_monitorThread != null)
                {
                    _monitorThread = null;
                }

                // The "Close()" will trigger RegNotifyChangeKeyValue if it is still listening
                if (MonitoredKey != null)
                {
                    MonitoredKey.Close();
                    MonitoredKey = null;
                    Logger.Instance.Information($"Stopped to listen to the registry key '{_registryPath}'");
                }
            }
        }

        /// <inheritdoc/>
        public void Resume()
        {
            lock (this)
            {
                if (_monitorThread == null)
                {
                    ThreadStart ts = RegistryMonitorThread;
                    _monitorThread = new Thread(ts);
                    _monitorThread.IsBackground = true;
                }

                if (!_monitorThread.IsAlive)
                {
                    _monitorThread.Start();
                    Logger.Instance.Information($"Started to listen to the registry key '{_registryPath}'");
                }
            }
        }

        /// <summary>
        /// Initialize the monitor
        /// </summary>
        private void Initialize()
        {
            lock (this)
            {
                if (_registryPath.StartsWith("HKEY_CLASSES_ROOT"))
                {
                    MonitoredKey = Registry.ClassesRoot.OpenSubKey(_registryPath.Substring(18));
                }
                else if (_registryPath.StartsWith("HKCR"))
                {
                    MonitoredKey = Registry.ClassesRoot.OpenSubKey(_registryPath.Substring(5));
                }
                else if (_registryPath.StartsWith("HKEY_CURRENT_USER"))
                {
                    MonitoredKey = Registry.CurrentUser.OpenSubKey(_registryPath.Substring(18));
                }
                else if (_registryPath.StartsWith("HKCU"))
                {
                    MonitoredKey = Registry.CurrentUser.OpenSubKey(_registryPath.Substring(5));
                }
                else if (_registryPath.StartsWith("HKEY_LOCAL_MACHINE"))
                {
                    MonitoredKey = Registry.LocalMachine.OpenSubKey(_registryPath.Substring(19));
                }
                else if (_registryPath.StartsWith("HKLM"))
                {
                    MonitoredKey = Registry.LocalMachine.OpenSubKey(_registryPath.Substring(5));
                }
                else if (_registryPath.StartsWith("HKEY_USERS"))
                {
                    MonitoredKey = Registry.Users.OpenSubKey(_registryPath.Substring(11));
                }
                else if (_registryPath.StartsWith("HKU"))
                {
                    MonitoredKey = Registry.Users.OpenSubKey(_registryPath.Substring(4));
                }
                else if (_registryPath.StartsWith("HKEY_CURRENT_CONFIG"))
                {
                    MonitoredKey = Registry.CurrentConfig.OpenSubKey(_registryPath.Substring(20));
                }
                else if (_registryPath.StartsWith("HKCC"))
                {
                    MonitoredKey = Registry.CurrentConfig.OpenSubKey(_registryPath.Substring(5));
                }

                if (MonitoredKey == null)
                {
                    throw new KeyNotFoundException($"The registry key '{_registryPath}' was not found.");
                }

                var hkey = typeof(RegistryKey).InvokeMember("hkey", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic, null, MonitoredKey, null);
                _monitoredKeyHandle = (IntPtr)typeof(SafeHandle).InvokeMember("handle", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic, null, hkey, null);
            }
        }

        /// <summary>
        /// Called by the monitor's thread to listen to the registry.
        /// </summary>
        private void RegistryMonitorThread()
        {
            try
            {
                if (_monitoredKeyHandle == IntPtr.Zero)
                {
                    return;
                }

                while (true)
                {
                    // If this._monitorThread is null that probably means Dispose is being called. Don't monitor anymore.
                    if ((_monitorThread == null) || (MonitoredKey == null))
                    {
                        break;
                    }

                    // RegNotifyChangeKeyValue blocks until a change occurs.
                    var result = NativeMethods.RegNotifyChangeKeyValue(_monitoredKeyHandle, true, _filter, IntPtr.Zero, false);

                    if ((_monitorThread == null) || (MonitoredKey == null))
                    {
                        break;
                    }

                    if (result == 0)
                    {
                        if (Changed != null)
                        {
                            lock (this)
                            {
                                Changed(this, new EventArgs());
                            }

                            if (_monitorThread == null)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        var ex = new Win32Exception();

                        // Unless the exception is thrown, nobody is nice enough to set a good stacktrace for us. Set it ourselves.
                        typeof(Exception).InvokeMember("_stackTrace", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField, null, ex, new object[] { new StackTrace(true) });

                        Logger.Instance.Error(ex);

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex);
            }
            finally
            {
                Pause();
            }
        }

        #endregion

        #region Destructors

        /// <summary>
        /// Explicit destructor of the class.
        /// </summary>
        ~RegistryMonitor()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose and unhook all hooks.
        /// </summary>
        public void Dispose()
        {
            Pause();
        }

        #endregion
    }
}
