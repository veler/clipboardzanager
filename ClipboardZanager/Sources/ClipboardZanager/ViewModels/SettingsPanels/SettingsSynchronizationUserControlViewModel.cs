using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClipboardZanager.ComponentModel.Messages;
using ClipboardZanager.ComponentModel.Services;
using ClipboardZanager.ComponentModel.UI.Controls;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Core.Desktop.Services;
using ClipboardZanager.Properties;
using ClipboardZanager.Shared.CloudStorage;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Shared.Services;
using ClipboardZanager.Strings;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace ClipboardZanager.ViewModels.SettingsPanels
{
    /// <summary>
    /// Interaction logic for <see cref="SettingsSynchronizationUserControl"/>
    /// </summary>
    internal sealed class SettingsSynchronizationUserControlViewModel : ViewModelBase
    {
        #region Fields

        private readonly ServiceSettingProvider _settingProvider;
        private readonly CloudStorageService _cloudStorageService;

        private bool _isLinkCloudStorageServicePopupOpened;
        private bool _isAuthenticatingCloudStorageService;
        private bool _isUnlinkCloudStorageServicePopupOpened;
        private bool _isLoadingInfo;
        private bool _connectionProblem;
        private string _currentCloudStorageServiceName;
        private string _currentCloudStorageServiceUserName;

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
        /// Gets or sets whether the software should synchronize data with the cloud over metered connection or not
        /// </summary>
        public bool AvoidMeteredConnection
        {
            get { return Settings.Default.AvoidMeteredConnection; }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(AvoidMeteredConnection)}' has been set to '{value}'.");
                Settings.Default.AvoidMeteredConnection = value;
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets the interval (in munute) between each synchronization
        /// </summary>
        public string SynchronizationInterval
        {
            get { return Settings.Default.SynchronizationInterval.ToString(); }
            set
            {
                if (int.TryParse(value, out int newInt))
                {
                    if (newInt < 1)
                    {
                        newInt = 1;
                    }
                    else if (newInt > 60)
                    {
                        newInt = 60;
                    }

                    Logger.Instance.Information($"The setting '{nameof(SynchronizationInterval)}' has been set to '{value}'.");
                    Settings.Default.SynchronizationInterval = newInt;
                }
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets a value that defines whether the software is linked to a cloud storage service.
        /// </summary>
        public bool IsLinkedToCloudStorageService => _cloudStorageService.IsLinkedToAService;

        /// <summary>
        /// Gets the list of providers to display in the UI.
        /// </summary>
        public IReadOnlyList<CloudStorageProvider> CloudStorageProviders => _cloudStorageService.CloudStorageProviders;

        /// <summary>
        /// Gets or sets a value that defines whether the popup for linking the app to a cloud storage service is opened
        /// </summary>
        public bool IsLinkCloudStorageServicePopupOpened
        {
            get { return _isLinkCloudStorageServicePopupOpened; }
            set
            {
                _isLinkCloudStorageServicePopupOpened = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value that defines whether the authentication to a cloud storage provider is in progress
        /// </summary>
        public bool IsAuthenticatingCloudStorageService
        {
            get { return _isAuthenticatingCloudStorageService; }
            set
            {
                _isAuthenticatingCloudStorageService = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value that defines whether the app is loading information about the cloud storage account
        /// </summary>
        public bool IsLoadingInfo
        {
            get { return _isLoadingInfo; }
            set
            {
                _isLoadingInfo = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the name of the current cloud storage provider.
        /// </summary>
        public string CurrentCloudStorageServiceName
        {
            get { return _currentCloudStorageServiceName; }
            set
            {
                _currentCloudStorageServiceName = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the user name of the current cloud storage provider.
        /// </summary>
        public string CurrentCloudStorageServiceUserName
        {
            get { return _currentCloudStorageServiceUserName; }
            set
            {
                _currentCloudStorageServiceUserName = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value that defines whether the popup for unlinking the app from a cloud storage service is opened
        /// </summary>
        public bool IsUnlinkCloudStorageServicePopupOpened
        {
            get { return _isUnlinkCloudStorageServicePopupOpened; }
            set
            {
                _isUnlinkCloudStorageServicePopupOpened = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value that defines whether the cloud storage account information have been retrieved correctly
        /// </summary>
        public bool ConnectionProblem
        {
            get { return _connectionProblem; }
            set
            {
                _connectionProblem = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="SettingsSynchronizationUserControlViewModel"/> class.
        /// </summary>
        internal SettingsSynchronizationUserControlViewModel()
        {
            _cloudStorageService = ServiceLocator.GetService<CloudStorageService>();
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
            LoadedCommand = new RelayCommand(ExecuteLoadedCommand);
            LinkToCloudStorageServiceCommand = new RelayCommand(ExecuteLinkToCloudStorageServiceCommand);
            StartLinkCloudCommand = new RelayCommand<List<object>>(ExecuteStartLinkCloudCommand);
            LinkToCloudStorageServicePopupClosedCommand = new RelayCommand<CloudStorageAuthenticationUserControl>(ExecuteLinkToCloudStorageServicePopupClosedCommand);
            UnlinkCommand = new RelayCommand(ExecuteUnlinkCommand);
            ConfirmUnlinkCommand = new RelayCommand(ExecuteConfirmUnlinkCommand);
            SynchronizeNowCommand = new RelayCommand(ExecuteSynchronizeNowCommand);
        }

        #region Loaded

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the <see cref="SettingsSynchronizationUserControl"/> is loaded
        /// </summary>
        public RelayCommand LoadedCommand { get; private set; }

        private void ExecuteLoadedCommand()
        {
            AuthenticateCloudAsync();
        }

        #endregion

        #region LinkToCloudStorageService

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when we click on the Link to a cloud storage service button
        /// </summary>
        public RelayCommand LinkToCloudStorageServiceCommand { get; private set; }

        private void ExecuteLinkToCloudStorageServiceCommand()
        {
            RaisePropertyChanged(string.Empty);
            IsLinkCloudStorageServicePopupOpened = true;
            MessengerInstance.Send(new Message(), MessageIdentifiers.LinkCloudStorageServicePopupOpened);
        }

        #endregion

        #region StartLinkCloud

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when we click on the Link to a cloud storage service button
        /// </summary>
        public RelayCommand<List<object>> StartLinkCloudCommand { get; private set; }

        private async void ExecuteStartLinkCloudCommand(List<object> parameters)
        {
            var provider = (CloudStorageProvider)parameters.First();
            var authenticationUserControl = (CloudStorageAuthenticationUserControl)parameters.Last();
            authenticationUserControl.AuthenticationCanceled += AuthenticationUserControl_AuthenticationCanceled;
            authenticationUserControl.AuthenticationCompleted += AuthenticationUserControl_AuthenticationCompleted;

            Logger.Instance.Information($"The will try to login to {provider.Name}.");

            await SignOutCloudAsync();

            DispatcherHelper.CreateNewThread().Invoke(() =>
            {
                AuthenticateCloudAsync(provider, authenticationUserControl);
            });
        }

        #endregion

        #region LinkToCloudStorageServicePopupClosed

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the popup designed to link the app to a cloud storage service is closed
        /// </summary>
        public RelayCommand<CloudStorageAuthenticationUserControl> LinkToCloudStorageServicePopupClosedCommand { get; private set; }

        private void ExecuteLinkToCloudStorageServicePopupClosedCommand(CloudStorageAuthenticationUserControl authenticationUserControl)
        {
            if (!IsAuthenticatingCloudStorageService)
            {
                IsLinkCloudStorageServicePopupOpened = false;
                return;
            }

            Requires.NotNull(authenticationUserControl, nameof(authenticationUserControl));
            IsAuthenticatingCloudStorageService = false;
            authenticationUserControl.AuthenticationCanceled -= AuthenticationUserControl_AuthenticationCanceled;
            authenticationUserControl.AuthenticationCompleted -= AuthenticationUserControl_AuthenticationCompleted;
        }

        #endregion

        #region Unlink

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when we click on the Unlink button
        /// </summary>
        public RelayCommand UnlinkCommand { get; private set; }

        private void ExecuteUnlinkCommand()
        {
            IsUnlinkCloudStorageServicePopupOpened = true;
            MessengerInstance.Send(new Message(), MessageIdentifiers.UnlinkCloudStorageServicePopupOpened);
        }

        #endregion

        #region ConfirmUnlink

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when we click on the Confirm button of the Unlink popup
        /// </summary>
        public RelayCommand ConfirmUnlinkCommand { get; private set; }

        private async void ExecuteConfirmUnlinkCommand()
        {
            Logger.Instance.Information($"The will sign out from the current cloud storage provider.");
            await SignOutCloudAsync();
            IsUnlinkCloudStorageServicePopupOpened = false;
            RaisePropertyChanged(nameof(IsLinkedToCloudStorageService));
            _settingProvider.SaveAndApplySettings();
        }

        #endregion

        #region SynchronizeNow

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when we click on the Synchronize Now button.
        /// </summary>
        public RelayCommand SynchronizeNowCommand { get; private set; }

        private async void ExecuteSynchronizeNowCommand()
        {
            Logger.Instance.Information($"The user wants to synchronize its data with the cloud right now.");
            IsLoadingInfo = true;

            try
            {
                await _cloudStorageService.SynchronizeAsync();
            }
            catch (Exception exception)
            {
                ConnectionProblem = true;
                Logger.Instance.Warning($"Failed to synchronize with {CurrentCloudStorageServiceName} : {exception.Message}");
            }

            IsLoadingInfo = false;
        }

        #endregion

        #endregion

        #region Handled Methods

        private void AuthenticationUserControl_AuthenticationCompleted(object sender, System.Windows.RoutedEventArgs e)
        {
            IsLinkCloudStorageServicePopupOpened = false;
            IsLoadingInfo = true;
        }

        private void AuthenticationUserControl_AuthenticationCanceled(object sender, System.Windows.RoutedEventArgs e)
        {
            if (LinkToCloudStorageServicePopupClosedCommand.CanExecute(sender))
            {
                LinkToCloudStorageServicePopupClosedCommand.Execute(sender);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Try to authenticate on a cloud storage provider with the UI and if it succeed, load the informations about the user.
        /// </summary>
        /// <param name="provider">The provider that the user chosen</param>
        /// <param name="authenticationUserControl">The authentication form</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task AuthenticateCloudAsync(CloudStorageProvider provider, ICloudAuthentication authenticationUserControl)
        {
            Requires.NotNull(provider, nameof(provider));
            Requires.NotNull(authenticationUserControl, nameof(authenticationUserControl));
            IsAuthenticatingCloudStorageService = true;

            Logger.Instance.Information($"Authentication to {provider.Name}.");
            if (await _cloudStorageService.GetProviderFromName(provider.Name).TryAuthenticateWithUiAsync(authenticationUserControl))
            {
                Logger.Instance.Information($"Authentication to {provider.Name} succeeded.");
                await LoadCloudUserInfoAsync();
            }
            else
            {
                Logger.Instance.Information($"Authentication to {provider.Name} failed.");
            }

            IsLoadingInfo = false;
            RaisePropertyChanged(nameof(IsLinkedToCloudStorageService));
            _settingProvider.SaveAndApplySettings();
        }

        /// <summary>
        /// Try to authenticate on a cloud storage provider with the user token from the previous/current session and if it succeed, load the informations about the user.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task AuthenticateCloudAsync()
        {
            if (!_cloudStorageService.IsLinkedToAService)
            {
                return;
            }

            IsLoadingInfo = true;
            await LoadCloudUserInfoAsync();
            IsLoadingInfo = false;

            RaisePropertyChanged(nameof(IsLinkedToCloudStorageService));
        }

        /// <summary>
        /// Load the information about the cloud storage user account
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task LoadCloudUserInfoAsync()
        {
            CurrentCloudStorageServiceName = _cloudStorageService.CurrentCloudStorageProvider.CloudServiceName;
            try
            {
                Logger.Instance.Information($"The user looks authenticated to {CurrentCloudStorageServiceName}. The app tries to retrieve the user's name on the cloud storage provider.");
                CurrentCloudStorageServiceUserName = await _cloudStorageService.CurrentCloudStorageProvider.GetUserNameAsync();
                Logger.Instance.Information($"Success to retrieve informations on {CurrentCloudStorageServiceName}'s account.");
            }
            catch
            {
                ConnectionProblem = true;
                Logger.Instance.Information($"Failed to retrieve informations on {CurrentCloudStorageServiceName}'s account.");
            }
        }

        /// <summary>
        /// Sign out in all the cloud storage providers
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task SignOutCloudAsync()
        {
            Logger.Instance.Information($"Signing out from all cloud storage service providers.");
            IsLoadingInfo = true;
            await _cloudStorageService.SignOutAllAsync();
            IsLoadingInfo = false;
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
