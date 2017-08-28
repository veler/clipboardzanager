using ClipboardZanager.ComponentModel.Messages;
using ClipboardZanager.ComponentModel.Services;
using ClipboardZanager.Properties;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Strings;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace ClipboardZanager.ViewModels.SettingsPanels
{
    /// <summary>
    /// Interaction logic for <see cref="SettingsNotificationsUserControl"/>
    /// </summary>
    internal sealed class SettingsNotificationsUserControlViewModel : ViewModelBase
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
        /// Gets or sets whether a notification should be displayed when the synchronization has failed
        /// </summary>
        public bool NotifySyncFailed
        {
            get { return Settings.Default.NotifySyncFailed; }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(NotifySyncFailed)}' has been set to '{value}'.");
                Settings.Default.NotifySyncFailed = value;
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets whether a notification should be displayed when a credit card number has been copied and detected
        /// </summary>
        public bool NotifyCreditCard
        {
            get { return Settings.Default.NotifyCreditCard; }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(NotifyCreditCard)}' has been set to '{value}'.");
                Settings.Default.NotifyCreditCard = value;
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets whether a notification should be displayed when a password has been copied and detected
        /// </summary>
        public bool NotifyPassword
        {
            get { return Settings.Default.NotifyPassword; }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(NotifyPassword)}' has been set to '{value}'.");
                Settings.Default.NotifyPassword = value;
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="SettingsNotificationsUserControlViewModel"/> class.
        /// </summary>
        internal SettingsNotificationsUserControlViewModel()
        {
            _settingProvider = new ServiceSettingProvider();

            Messenger.Default.Register<Message>(this, MessageIdentifiers.RaisePropertyChangedOnAllSettingsUserControl, RaiseAllPropertyChanged);
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
