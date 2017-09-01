using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using ClipboardZanager.ComponentModel.Services;
using ClipboardZanager.ComponentModel.UI.Controls;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Properties;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Strings;
using ClipboardZanager.ViewModels.SettingsPanels;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using ClipboardZanager.Shared.Services;
using ClipboardZanager.Core.Desktop.Services;

namespace ClipboardZanager.ViewModels
{
    /// <summary>
    /// Interaction logic for <see cref="FirstStartWindow"/>
    /// </summary>
    internal sealed class FirstStartWindowViewModel : SettingsSecurityUserControlViewModel
    {
        #region Fields

        private readonly ServiceSettingProvider _settingProvider;
        private string _migrationStatus;
        private int _migrationProgress;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="LanguageManager"/>
        /// </summary>
        public LanguageManager Language => LanguageManager.GetInstance();

        /// <summary>
        /// Gets a value that defines whether a screen reader is running
        /// </summary>
        public bool IsScreenReaderRunning => SystemInfoHelper.IsScreenReaderRunning();

        /// <summary>
        /// Gets the list of available cultures in the application
        /// </summary>
        public List<CultureInfo> AvailableLanguages => Language.GetAvailableCultures();

        /// <summary>
        /// Gets or sets the current language of the application
        /// </summary>
        public string CurrentLanguage
        {
            get { return Settings.Default.Language; }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(CurrentLanguage)}' has been set to '{value}'.");
                Settings.Default.Language = value;
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();

                LoadNews();
            }
        }

        /// <summary>
        /// Gets the version of the executable.
        /// </summary>
        public string ApplicationVersion => CoreHelper.GetApplicationVersion().ToString();

        /// <summary>
        /// Gets the list of news to display in the What's new view.
        /// </summary>
        public ObservableCollection<SoftwareNewItem> News { get; }

        /// <summary>
        /// Gets or sets the message that corresponds to the data migration status.
        /// </summary>
        public string MigrationStatus
        {
            get { return _migrationStatus; }
            set
            {
                _migrationStatus = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the percent of progression of the migration of the data.
        /// </summary>
        public int MigrationProgress
        {
            get { return _migrationProgress; }
            set
            {
                _migrationProgress = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value that defines whether a migration is required. It usually means that the current instance is the first start after an update.
        /// </summary>
        public bool IsMigrationRequired { get; }

        #endregion 

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="FirstStartWindowViewModel"/> class.
        /// </summary>
        internal FirstStartWindowViewModel()
        {
            InitializeCommands();

            _settingProvider = new ServiceSettingProvider();

            IsMigrationRequired = !string.IsNullOrWhiteSpace(Settings.Default.CurrentVersion) && Settings.Default.DataMigrationRequired;
            News = new ObservableCollection<SoftwareNewItem>();
            LoadNews();

            if (!IsMigrationRequired)
            {
                DetectPasswordsManager();
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Initialize the commands of the View Model
        /// </summary>
        private void InitializeCommands()
        {
            LocalLoadedCommand = new RelayCommand(ExecuteLoadedCommand);
            CloseButtonCommand = new RelayCommand<BlurredWindow>(ExecuteCloseButtonCommand);
        }

        #region Loaded

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the <see cref="FirstStartWindow"/> is loaded
        /// </summary>
        public RelayCommand LocalLoadedCommand { get; private set; }

        private void ExecuteLoadedCommand()
        {
            ServiceLocator.GetService<DataService>().DataMigrationProgress += DataService_DataMigrationProgress;
        }

        #endregion

        #region CloseButton

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when click on the close button
        /// </summary>
        public RelayCommand<BlurredWindow> CloseButtonCommand { get; private set; }

        private void ExecuteCloseButtonCommand(BlurredWindow window)
        {
            Logger.Instance.Information($"The first start window has been closed.");
            window.Close();
        }

        #endregion

        #endregion

        #region Handled Methods

        private void DataService_DataMigrationProgress(object sender, Core.Desktop.Events.DataMigrationProgressEventArgs e)
        {
            MigrationProgress = e.Percent;

            if (e.Completed)
            {
                if (e.Failed)
                {
                    MigrationStatus = Language.FirstStartWindow.MigrationFailed;
                }
                else
                {
                    MigrationStatus = Language.FirstStartWindow.MigrationCompleted;
                }
            }
            else
            {
                MigrationStatus = Language.FirstStartWindow.MigrationInProgress;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load the data to display in the "What's new" part.
        /// </summary>
        private void LoadNews()
        {
            var resource = Application.GetResourceStream(new Uri($"/ClipboardZanager;component/Assets/news/news.{CurrentLanguage}.json", UriKind.RelativeOrAbsolute));

            News.Clear();
            if (resource != null)
            {
                foreach (var softwareNewItem in JsonConvert.DeserializeObject<ObservableCollection<SoftwareNewItem>>(new StreamReader(resource.Stream).ReadToEnd()))
                {
                    News.Add(softwareNewItem);
                }
            }
        }

        private void DetectPasswordsManager()
        {
            var processes = Process.GetProcesses();
            var passwordManagerList = new Dictionary<string, string>
            {
                // Process name, Application title in the ignored list
                { "PasswordZanager", "PasswordZanager" },
                { "AgileBits.OnePassword.Desktop", "1Password" },
                { "Dashlane", "Dashlane" },
                { "KeePass", "KeePass Password Safe" }
            };

            foreach (var process in processes)
            {
                var passwordManagerDetected = passwordManagerList.FirstOrDefault(item => string.Equals(item.Key, process.ProcessName, StringComparison.OrdinalIgnoreCase));
                if (!passwordManagerDetected.Equals(default(KeyValuePair<string, string>)))
                {
                    var ignoredApp = new IgnoredApplication();
                    var applicationIdentifier = SystemInfoHelper.GetExecutablePath(process.Id);

                    ignoredApp.DisplayName = passwordManagerDetected.Value;
                    ignoredApp.ApplicationIdentifier = applicationIdentifier;

                    if (File.Exists(applicationIdentifier))
                    {
                        var tempIcon = Icon.ExtractAssociatedIcon(applicationIdentifier)?.ToBitmap();
                        if (tempIcon != null)
                        {
                            ignoredApp.Icon = DataHelper.BitmapToBitmapImage(new Bitmap(tempIcon, Consts.WindowsIconsSize, Consts.WindowsIconsSize), Consts.WindowsIconsSize);
                        }
                    }

                    var collection = IgnoredApplications;
                    if (collection.Any(app => app.DisplayName == ignoredApp.DisplayName))
                    {
                        continue;
                    }

                    collection.Add(ignoredApp);
                    IgnoredApplications = collection;
                    Logger.Instance.Information($"The application '{ignoredApp.ApplicationIdentifier}' has been added to the ignored app list.");
                }
            }
        }

        #endregion
    }
}
