using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using ClipboardZanager.ComponentModel.Messages;
using ClipboardZanager.ComponentModel.Services;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Core.Desktop.Services;
using ClipboardZanager.Properties;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Shared.Services;
using ClipboardZanager.Strings;
using ClipboardZanager.Views.SettingsPanels;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;

namespace ClipboardZanager.ViewModels.SettingsPanels
{
    /// <summary>
    /// Interaction logic for <see cref="SettingsSecurityUserControl"/>
    /// </summary>
    internal class SettingsSecurityUserControlViewModel : ViewModelBase
    {
        #region Fields

        private readonly WindowsService _windowsService;
        private readonly ServiceSettingProvider _settingProvider;

        private ObservableCollection<Window> _windowsList;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="LanguageManager"/>
        /// </summary>
        public LanguageManager Language => LanguageManager.GetInstance();

        /// <summary>
        /// Gets or sets whether the credit card numbers and password must be synchronized when it is detected
        /// </summary>
        public bool DisablePasswordAndCreditCardSync
        {
            get { return Settings.Default.DisablePasswordAndCreditCardSync; }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(DisablePasswordAndCreditCardSync)}' has been set to '{value}'.");
                Settings.Default.DisablePasswordAndCreditCardSync = value;
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets whether the credit card numbers must be captured when it is detected.
        /// </summary>
        public bool AvoidCreditCard
        {
            get { return Settings.Default.AvoidCreditCard; }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(AvoidCreditCard)}' has been set to '{value}'.");
                Settings.Default.AvoidCreditCard = value;
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets whether the passwords must be captured when it is detected.
        /// </summary>
        public bool AvoidPasswords
        {
            get { return Settings.Default.AvoidPasswords; }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(AvoidPasswords)}' has been set to '{value}'.");
                Settings.Default.AvoidPasswords = value;
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets the list of ignored applications
        /// </summary>
        public ObservableCollection<IgnoredApplication> IgnoredApplications
        {
            get { return JsonConvert.DeserializeObject<ObservableCollection<IgnoredApplication>>(Settings.Default.IgnoredApplications); }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(IgnoredApplications)}' has been set to '{value}'.");
                Settings.Default.IgnoredApplications = JsonConvert.SerializeObject(value);
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="Window"/> that corresponds to the current displayed windows in the OS.
        /// </summary>
        public ObservableCollection<Window> WindowsList
        {
            get { return _windowsList; }
            set
            {
                _windowsList = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="SettingsSecurityUserControlViewModel"/> class.
        /// </summary>
        internal SettingsSecurityUserControlViewModel()
        {
            if (!IsInDesignMode)
            {
                _windowsService = ServiceLocator.GetService<WindowsService>();
            }

            _settingProvider = new ServiceSettingProvider();

            InitializeCommands();

            Messenger.Default.Register<Message>(this, MessageIdentifiers.RaisePropertyChangedOnAllSettingsUserControl, RaiseAllPropertyChanged);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Initialize the commands of the View Model
        /// </summary>
        private void InitializeCommands()
        {
            LoadedCommand = new RelayCommand<System.Windows.Window>(ExecuteLoadedCommand);
            UnloadedCommand = new RelayCommand<System.Windows.Window>(ExecuteUnloadedCommand);
            DeleteIgnoredApplicationCommand = new RelayCommand<IgnoredApplication>(ExecuteDeleteIgnoredApplicationCommand);
            AddWindowToIgnoreListCommand = new RelayCommand<Window>(ExecuteAddWindowToIgnoreListCommand);
        }

        #region Loaded

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the <see cref="SettingsSecurityUserControl"/> is loaded
        /// </summary>
        public RelayCommand<System.Windows.Window> LoadedCommand { get; private set; }

        private void ExecuteLoadedCommand(System.Windows.Window window)
        {
            Requires.NotNull(window, nameof(window));
            _windowsService.AddWindowToIgnoreList(window);
            _windowsService.WindowsListChanged += WindowsService_WindowsListChanged;
            _windowsService.RefreshWindows();
        }

        #endregion

        #region Unloaded

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the <see cref="SettingsSecurityUserControl"/> is unloaded
        /// </summary>
        public RelayCommand<System.Windows.Window> UnloadedCommand { get; private set; }

        private void ExecuteUnloadedCommand(System.Windows.Window window)
        {
            Requires.NotNull(window, nameof(window));
            _windowsService.RemoveWindowToIgnoreList(window);
            _windowsService.WindowsListChanged -= WindowsService_WindowsListChanged;
        }

        #endregion  

        #region DeleteIgnoredApplication

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the user delete an ignored application
        /// </summary>
        public RelayCommand<IgnoredApplication> DeleteIgnoredApplicationCommand { get; private set; }

        private void ExecuteDeleteIgnoredApplicationCommand(IgnoredApplication ignoredApp)
        {
            var collection = IgnoredApplications;
            collection.Remove(collection.Single(app => app.ApplicationIdentifier == ignoredApp.ApplicationIdentifier));
            IgnoredApplications = collection;
            Logger.Instance.Information($"The application '{ignoredApp.ApplicationIdentifier}' has been removed from the ignored app list.");
        }

        #endregion

        #region AddWindowToIgnoreList

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the user select a window to add to the applications ignored list
        /// </summary>
        public RelayCommand<Window> AddWindowToIgnoreListCommand { get; private set; }

        private void ExecuteAddWindowToIgnoreListCommand(Window window)
        {
            if (window == null || window.Handle == IntPtr.Zero)
            {
                return;
            }

            var collection = IgnoredApplications;
            var ignoredApp = new IgnoredApplication
            {
                ApplicationIdentifier = window.ApplicationIdentifier
            };

            if (window.IsWindowsStoreApp)
            {
                ignoredApp.Icon = window.Icon;
            }
            else if (File.Exists(window.ApplicationIdentifier))
            {
                var icon = Icon.ExtractAssociatedIcon(window.ApplicationIdentifier)?.ToBitmap();

                if (icon != null)
                {
                    ignoredApp.Icon = DataHelper.BitmapToBitmapImage(new Bitmap(icon, Consts.WindowsIconsSize, Consts.WindowsIconsSize), Consts.WindowsIconsSize);
                }
                else
                {
                    ignoredApp.Icon = window.Icon;
                }
            }
            else
            {
                ignoredApp.Icon = window.Icon;
            }

            if (window.IsWindowsStoreApp)
            {
                ignoredApp.DisplayName = window.ApplicationIdentifier.Split('_').First();
            }
            else
            {
                ignoredApp.DisplayName = Path.GetFileNameWithoutExtension(window.ApplicationIdentifier);
            }

            if (collection.Any(app => app.DisplayName == ignoredApp.DisplayName))
            {
                return;
            }

            collection.Add(ignoredApp);
            IgnoredApplications = collection;

            WindowsList.Remove(window);
            Logger.Instance.Information($"The application '{ignoredApp.ApplicationIdentifier}' has been added to the ignored app list.");
        }

        #endregion

        #endregion

        #region Handled Methods

        private void WindowsService_WindowsListChanged(object sender, EventArgs e)
        {
            WindowsList = new ObservableCollection<Window>(_windowsService.WindowsList);
            WindowsList.Insert(0, new Window(IntPtr.Zero, string.Empty, null, string.Empty, null, false));
        }

        #endregion

        #region Methods

        /// <summary>
        /// After a "Restore Default Settings", raise all the property changed of the user control to update the view.
        /// </summary>
        /// <param name="message">The message.</param>
        private void RaiseAllPropertyChanged(Message message)
        {
            RaisePropertyChanged(string.Empty);
        }

        #endregion
    }
}
