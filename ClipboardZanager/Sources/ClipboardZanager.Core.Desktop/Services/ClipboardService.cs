using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClipboardZanager.Core.Desktop.Clipboard;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Hooking;
using ClipboardZanager.Core.Desktop.IO;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Shared.Services;
using Application = System.Windows.Application;
using Window = System.Windows.Window;

namespace ClipboardZanager.Core.Desktop.Services
{
    /// <summary>
    /// Provides a service that can listen to the clipboard, read an write it.
    /// </summary>
    internal sealed class ClipboardService : IService, IPausable
    {
        #region Fields

        private bool _isPaused = true;
        private bool _ignoreClipboardChange;
        private ClipboardHook _clipboardHook;
        private List<IgnoredApplication> _ignoredApplications;
        private IServiceSettingProvider _settingProvider;

        #endregion

        #region Handled Methods

        /// <summary>
        /// Called when the clipboard content has changed.
        /// </summary>
        /// <param name="sender">The <see cref="ClipboardHook"/>.</param>
        /// <param name="e">The information about the changes.</param>
        internal void ClipboardHook_ClipboardChanged(object sender, Events.ClipboardHookEventArgs e)
        {
            if (_ignoreClipboardChange)
            {
                return;
            }

            _ignoreClipboardChange = true;

            var mouseAndKeyboardHookService = ServiceLocator.GetService<MouseAndKeyboardHookService>();
            mouseAndKeyboardHookService.Pause();
            Pause();

            var delayer = new Delayer<object>(TimeSpan.FromMilliseconds(10));
            delayer.Action += (o, args) =>
            {
                try
                {
                    Logger.Instance.Information($"A data has been copied. The {nameof(ClipboardService)} tries to retrieve the data.");

                    Requires.NotNull(e, nameof(e));
                    DispatcherHelper.ThrowIfNotStaThread();

                    var dataIgnored = false;
                    var isCreditCard = false;
                    var isPassword = false;

                    var dataService = ServiceLocator.GetService<DataService>();
                    var foregroundWindow = ServiceLocator.GetService<WindowsService>().GetForegroundWindow();

                    if (_ignoredApplications.Any(app => app.ApplicationIdentifier == foregroundWindow.ApplicationIdentifier))
                    {
                        dataIgnored = true;
                        Logger.Instance.Information($"The {nameof(ClipboardService)} ignored the data because it comes from an ignored application.");
                    }
                    else
                    {
                        if (e.DataObject.ContainsText())
                        {
                            var text = e.DataObject.GetText();

                            isCreditCard = dataService.IsCreditCard(text);
                            if (isCreditCard && dataService.KeepOrIgnoreCreditCard(text))
                            {
                                dataIgnored = true;
                                Logger.Instance.Information($"The {nameof(ClipboardService)} ignored the data because it is a credit card number.");
                            }

                            isPassword = dataService.IsPassword(text, foregroundWindow);
                            if (isPassword && dataService.KeepOrIgnorePassword(text))
                            {
                                dataIgnored = true;
                                Logger.Instance.Information($"The {nameof(ClipboardService)} ignored the data because it is a password.");
                            }
                        }
                    }

                    if (!dataIgnored)
                    {
                        dataService.Reset();

                        var identifiers = dataService.GetDataIdentifiers(e.DataObject.GetFormats());

                        if (identifiers.Count == 0)
                        {
                            Logger.Instance.Information($"The {nameof(ClipboardService)} ignored the data because it does not detected any compatible data format in the clipboard.");
                        }
                        else
                        {
                            using (Logger.Instance.Stopwatch("Persisting clipboard data to the hard drive."))
                            {
                                dataService.EnsureDataFolderExists();
                                using (var clipboardReader = new ClipboardReader(e.DataObject))
                                {
                                    foreach (var format in identifiers)
                                    {
                                        WriteClipboardDataToFile(clipboardReader, format, dataService.ClipboardDataPath);
                                    }
                                }
                            }

                            using (Logger.Instance.Stopwatch("Adding a new data entry to the data service."))
                            {
                                dataService.AddDataEntry(e, identifiers, foregroundWindow, isCreditCard, isPassword);
                            }

                            Logger.Instance.Information($"The {nameof(ClipboardService)} successfully retrieved the data from the clipboard.");
                        }
                    }
                }
                catch (Exception exception)
                {
                    Logger.Instance.Error(exception);
                }
                finally
                {
                    delayer = new Delayer<object>(TimeSpan.FromMilliseconds(10));
                    delayer.Action += (sender1, eventArgs) =>
                    {
                        _ignoreClipboardChange = false;
                        Resume();
                        mouseAndKeyboardHookService.Resume();
                        CoreHelper.MinimizeFootprint();
                    };
                    delayer.ResetAndTick();

                    if (CoreHelper.IsUnitTesting())
                    {
                        Task.Delay(10).Wait();
                        DispatcherHelper.DoEvents();
                    }
                }
            };
            delayer.ResetAndTick();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void Initialize(IServiceSettingProvider settingProvider)
        {
            _settingProvider = settingProvider;
            _ignoredApplications = _settingProvider.GetSetting<List<IgnoredApplication>>("IgnoredApplications");

            if (CoreHelper.IsUnitTesting())
            {
                _clipboardHook = new ClipboardHook(new Window());
            }
            else
            {
                _clipboardHook = new ClipboardHook(Application.Current.MainWindow);
            }

            _clipboardHook.ClipboardChanged += ClipboardHook_ClipboardChanged;

            Resume();

            Logger.Instance.Information($"{GetType().Name} initialized.");
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _clipboardHook.ClipboardChanged -= ClipboardHook_ClipboardChanged;
            _clipboardHook.Dispose();
        }

        /// <inheritdoc/>
        public void Pause()
        {
            _clipboardHook.Pause();
            _isPaused = true;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            Pause();

            _ignoredApplications = _settingProvider.GetSetting<List<IgnoredApplication>>("IgnoredApplications");

            if (!CoreHelper.IsUnitTesting())
            {
                Resume();
            }
        }

        /// <inheritdoc/>
        public void Resume()
        {
            if (_isPaused)
            {
                _clipboardHook.Resume();
            }
            _isPaused = false;
        }

        /// <summary>
        /// Set the clipboard content from a list of identifiers.
        /// </summary>
        /// <param name="identifiers">The list of <see cref="DataIdentifier"/> that specify which data must be added to the clipboard.</param>
        internal void SetClipboard(IEnumerable<DataIdentifier> identifiers)
        {
            Requires.NotNull(identifiers, nameof(identifiers));

            Pause();
            var dataService = ServiceLocator.GetService<DataService>();

            using (var clipboardWriter = new ClipboardWriter())
            {
                foreach (var dataIdentifier in identifiers)
                {
                    ReadFileToClipboardData(clipboardWriter, dataIdentifier, dataService.ClipboardDataPath);
                }

                clipboardWriter.Flush();
            }
        }

        /// <summary>
        /// Perform a Ctrl + V to paste the current data in the clipboard.
        /// </summary>
        internal void Paste()
        {
            Pause();

            var delayer = new Delayer<object>(TimeSpan.FromMilliseconds(100));
            delayer.Action += (sender, args) =>
            {
                SendKeys.SendWait("^v"); // Ctrl + V

                delayer = new Delayer<object>(TimeSpan.FromMilliseconds(100));
                delayer.Action += (sender2, args2) =>
                {
                    Resume();
                };
                delayer.ResetAndTick();
            };
            delayer.ResetAndTick();
        }

        /// <summary>
        /// Read a specified clipboard data, encrypt and save it into a file.
        /// </summary>
        /// <param name="clipboardReader">The <see cref="ClipboardReader"/> used to read the data.</param>
        /// <param name="identifier">The data identifier.</param>
        /// <param name="clipboardDataPath">The full path to the clipboard data folder.</param>
        private void WriteClipboardDataToFile(ClipboardReader clipboardReader, DataIdentifier identifier, string clipboardDataPath)
        {
            Requires.NotNull(clipboardReader, nameof(clipboardReader));
            Requires.NotNull(identifier, nameof(identifier));
            Requires.NotNullOrWhiteSpace(clipboardDataPath, nameof(clipboardDataPath));

            var dataFilePath = Path.Combine(clipboardDataPath, $"{identifier.Identifier}.dat");

            if (File.Exists(dataFilePath))
            {
                Logger.Instance.Fatal(new FileLoadException($"The file {dataFilePath} already exists."));
            }

            var dataPassword = SecurityHelper.ToSecureString(SecurityHelper.EncryptString(SecurityHelper.ToSecureString(identifier.Identifier.ToString())));
            Requires.NotNull(dataPassword, nameof(dataPassword));

            clipboardReader.BeginRead(identifier.FormatName);
            Requires.IsTrue(clipboardReader.IsReadable);
            Requires.IsTrue(clipboardReader.CanReadNextBlock());

            using (var fileStream = File.OpenWrite(dataFilePath))
            using (var aesStream = new AesStream(fileStream, dataPassword, SecurityHelper.GetSaltKeys(dataPassword).GetBytes(16)))
            {
                aesStream.AutoDisposeBaseStream = false;
                while (clipboardReader.CanReadNextBlock())
                {
                    var buffer = clipboardReader.ReadNextBlock();
                    aesStream.Write(buffer, 0, buffer.Length);
                }
                aesStream.Position = 0;
            }

            clipboardReader.EndRead();
        }

        /// <summary>
        /// Write a specified clipboard data from an encrypted file.
        /// </summary>
        /// <param name="clipboardWriter">The <see cref="ClipboardWriter"/> used to write the data.</param>
        /// <param name="identifier">The data identifier.</param>
        /// <param name="clipboardDataPath">The full path to the clipboard data folder.</param>
        private void ReadFileToClipboardData(ClipboardWriter clipboardWriter, DataIdentifier identifier, string clipboardDataPath)
        {
            Requires.NotNull(clipboardWriter, nameof(clipboardWriter));
            Requires.NotNull(identifier, nameof(identifier));
            Requires.NotNullOrWhiteSpace(clipboardDataPath, nameof(clipboardDataPath));

            var dataFilePath = Path.Combine(clipboardDataPath, $"{identifier.Identifier}.dat");

            if (!File.Exists(dataFilePath))
            {
                return;
            }

            var dataPassword = SecurityHelper.ToSecureString(SecurityHelper.EncryptString(SecurityHelper.ToSecureString(identifier.Identifier.ToString())));
            Requires.NotNull(dataPassword, nameof(dataPassword));

            var fileStream = File.OpenRead(dataFilePath);
            var aesStream = new AesStream(fileStream, dataPassword, SecurityHelper.GetSaltKeys(dataPassword).GetBytes(16));

            clipboardWriter.AddData(identifier.FormatName, aesStream, fileStream);
        }

        #endregion
    }
}
