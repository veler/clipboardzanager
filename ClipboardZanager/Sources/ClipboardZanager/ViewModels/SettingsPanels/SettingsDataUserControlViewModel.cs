using ClipboardZanager.ComponentModel.Messages;
using ClipboardZanager.ComponentModel.Services;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Properties;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Strings;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace ClipboardZanager.ViewModels.SettingsPanels
{
    /// <summary>
    /// Interaction logic for <see cref="SettingsDataUserControl"/>
    /// </summary>
    internal sealed class SettingsDataUserControlViewModel : ViewModelBase
    {
        #region Fields

        private readonly ServiceSettingProvider _settingProvider;

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
        /// Gets or sets whether the data must be kept after reboot or not
        /// </summary>
        public bool KeepDataAfterReboot
        {
            get { return Settings.Default.KeepDataAfterReboot; }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(KeepDataAfterReboot)}' has been set to '{value}'.");
                Settings.Default.KeepDataAfterReboot = value;
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets whether the text data must be kept or not
        /// </summary>
        public bool KeepTextData
        {
            get { return Settings.Default.KeepDataTypes.Contains((int)SupportedDataType.Text); }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(KeepTextData)}' has been set to '{value}'.");
                AddOrRemoveDataTypeToKeep(value, SupportedDataType.Text);
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets whether the images must be kept or not
        /// </summary>
        public bool KeepImagesData
        {
            get { return Settings.Default.KeepDataTypes.Contains((int)SupportedDataType.Image); }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(KeepImagesData)}' has been set to '{value}'.");
                AddOrRemoveDataTypeToKeep(value, SupportedDataType.Image);
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets whether the files must be kept or not
        /// </summary>
        public bool KeepFilesData
        {
            get { return Settings.Default.KeepDataTypes.Contains((int)SupportedDataType.Files); }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(KeepFilesData)}' has been set to '{value}'.");
                AddOrRemoveDataTypeToKeep(value, SupportedDataType.Files);
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets whether the Word must be kept or not
        /// </summary>
        public bool KeepMicrosoftWordData
        {
            get { return Settings.Default.KeepDataTypes.Contains((int)SupportedDataType.MicrosoftWord); }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(KeepMicrosoftWordData)}' has been set to '{value}'.");
                AddOrRemoveDataTypeToKeep(value, SupportedDataType.MicrosoftWord);
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets whether the Excel must be kept or not
        /// </summary>
        public bool KeepMicrosoftExcelData
        {
            get { return Settings.Default.KeepDataTypes.Contains((int)SupportedDataType.MicrosoftExcel); }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(KeepMicrosoftExcelData)}' has been set to '{value}'.");
                AddOrRemoveDataTypeToKeep(value, SupportedDataType.MicrosoftExcel);
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets whether the PowerPoint must be kept or not
        /// </summary>
        public bool KeepMicrosoftPowerPointData
        {
            get { return Settings.Default.KeepDataTypes.Contains((int)SupportedDataType.MicrosoftPowerPoint); }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(KeepMicrosoftPowerPointData)}' has been set to '{value}'.");
                AddOrRemoveDataTypeToKeep(value, SupportedDataType.MicrosoftPowerPoint);
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets whether the Outlook must be kept or not
        /// </summary>
        public bool KeepMicrosoftOutlookData
        {
            get { return Settings.Default.KeepDataTypes.Contains((int)SupportedDataType.MicrosoftOutlook); }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(KeepMicrosoftOutlookData)}' has been set to '{value}'.");
                AddOrRemoveDataTypeToKeep(value, SupportedDataType.MicrosoftOutlook);
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets whether the Photoshop must be kept or not
        /// </summary>
        public bool KeepAdobePhotoshopData
        {
            get { return Settings.Default.KeepDataTypes.Contains((int)SupportedDataType.AdobePhotoshop); }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(KeepAdobePhotoshopData)}' has been set to '{value}'.");
                AddOrRemoveDataTypeToKeep(value, SupportedDataType.AdobePhotoshop);
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets the maximum count of data that can be kept at the same time
        /// </summary>
        public string MaxDataToKeep
        {
            get { return Settings.Default.MaxDataToKeep.ToString(); }
            set
            {
                int newInt;
                if (int.TryParse(value, out newInt))
                {
                    if (newInt < 1)
                    {
                        newInt = 1;
                    }
                    else if (newInt > 100)
                    {
                        newInt = 100;
                    }

                    Logger.Instance.Information($"The setting '{nameof(MaxDataToKeep)}' has been set to '{value}'.");
                    Settings.Default.MaxDataToKeep = newInt;
                }

                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets the number of days after what a data must be deleted
        /// </summary>
        public string DateExpireLimit
        {
            get { return Settings.Default.DateExpireLimit.ToString(); }
            set
            {
                int newInt;
                if (int.TryParse(value, out newInt))
                {
                    if (newInt < 1)
                    {
                        newInt = 1;
                    }
                    else if (newInt > 90)
                    {
                        newInt = 90;
                    }

                    Logger.Instance.Information($"The setting '{nameof(DateExpireLimit)}' has been set to '{value}'.");
                    Settings.Default.DateExpireLimit = newInt;
                }

                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        #endregion 

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="SettingsDataUserControlViewModel"/> class.
        /// </summary>
        internal SettingsDataUserControlViewModel()
        {
            _settingProvider = new ServiceSettingProvider();

            Messenger.Default.Register<Message>(this, MessageIdentifiers.RaisePropertyChangedOnAllSettingsUserControl, RaiseAllPropertyChanged);
        }  

        #endregion

        #region Methods

        /// <summary>
        /// Add or remove a supported data type to keep to the application's settings
        /// </summary>
        /// <param name="keep">Defines whether the data must be added to the array or not</param>
        /// <param name="type">The <see cref="SupportedDataType"/> to add or remove</param>
        private void AddOrRemoveDataTypeToKeep(bool keep, SupportedDataType type)
        {
            var data = (int)type;
            if (keep && !Settings.Default.KeepDataTypes.Contains(data))
            {
                Settings.Default.KeepDataTypes.Add(data);
            }
            else if (Settings.Default.KeepDataTypes.Contains(data))
            {
                Settings.Default.KeepDataTypes.Remove(data);
            }
        }

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
