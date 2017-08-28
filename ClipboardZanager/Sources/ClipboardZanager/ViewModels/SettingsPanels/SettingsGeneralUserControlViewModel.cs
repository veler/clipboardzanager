using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;
using ClipboardZanager.ComponentModel.Enums;
using ClipboardZanager.ComponentModel.Messages;
using ClipboardZanager.ComponentModel.Services;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.Events;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Core.Desktop.Services;
using ClipboardZanager.Properties;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Shared.Services;
using ClipboardZanager.Strings;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;

namespace ClipboardZanager.ViewModels.SettingsPanels
{

    /// <summary>
    /// Interaction logic for <see cref="SettingsGeneralUserControl"/>
    /// </summary>
    internal sealed class SettingsGeneralUserControlViewModel : ViewModelBase
    {
        #region Fields

        private readonly List<Key> _keyboardShortcut;
        private readonly MouseAndKeyboardHookService _mouseAndKeyboardHookService;
        private readonly ServiceSettingProvider _settingProvider;

        private bool _isChangeHotKeyPopupOpened;
        private bool _isRestoreDefaultPopupOpened;
        private string _displayedCurrentKeyboardShortcut;
        private string _displayedTemporaryKeyboardShortcut;

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
            }
        }

        /// <summary>
        /// Gets or sets whether the UI can be displayed with a mouse gesture
        /// </summary>
        public bool StartWithWindows
        {
            get { return File.Exists(Consts.StartWithWindowsShortcutFileName); }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(StartWithWindows)}' has been set to '{value}'.");
                CoreHelper.UpdateStartWithWindowsShortcut(value);
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets whether the UI should appear on the top or the bottom of the screen.
        /// </summary>
        public PasteBarPosition PasteBarPosition
        {
            get { return Settings.Default.PasteBarPosition; }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(PasteBarPosition)}' has been set to '{value}'.");
                Settings.Default.PasteBarPosition = value;
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets whether the UI can be displayed with a mouse gesture
        /// </summary>
        public bool MouseGesture
        {
            get { return Settings.Default.UseMouseGesture; }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(MouseGesture)}' has been set to '{value}'.");
                Settings.Default.UseMouseGesture = value;
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets whether the UI can be displayed with a keyboard gesture
        /// </summary>
        public bool KeyboardGesture
        {
            get { return Settings.Default.UseKeyboardHotKeys; }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(KeyboardGesture)}' has been set to '{value}'.");
                Settings.Default.UseKeyboardHotKeys = value;
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets whether the UI can be closed when the mouse moves away.
        /// </summary>
        public bool ClosePasteBarWhenMouseIsAway
        {
            get { return Settings.Default.ClosePasteBarWhenMouseIsAway; }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(ClosePasteBarWhenMouseIsAway)}' has been set to '{value}'.");
                Settings.Default.ClosePasteBarWhenMouseIsAway = value;
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets whether the UI can be closed with the same keyboard shortcut than to open the UI.
        /// </summary>
        public bool ClosePasteBarWithHotKey
        {
            get { return Settings.Default.ClosePasteBarWithHotKey; }
            set
            {
                Logger.Instance.Information($"The setting '{nameof(ClosePasteBarWithHotKey)}' has been set to '{value}'.");
                Settings.Default.ClosePasteBarWithHotKey = value;
                RaisePropertyChanged();
                _settingProvider.SaveAndApplySettings();
            }
        }

        /// <summary>
        /// Gets or sets a value that defines whether the popup for changing keyboard shortcut is opened
        /// </summary>
        public bool IsChangeHotKeyPopupOpened
        {
            get { return _isChangeHotKeyPopupOpened; }
            set
            {
                _isChangeHotKeyPopupOpened = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value that represents the current keyboard shortcut
        /// </summary>
        public string DisplayedCurrentKeyboardShortcut
        {
            get { return _displayedCurrentKeyboardShortcut; }
            set
            {
                _displayedCurrentKeyboardShortcut = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value that represents the typed keyboard shortcut
        /// </summary>
        public string DisplayedTemporaryKeyboardShortcut
        {
            get { return _displayedTemporaryKeyboardShortcut; }
            set
            {
                _displayedTemporaryKeyboardShortcut = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value that defines whether the popup for restoring default settings is opened
        /// </summary>
        public bool IsRestoreDefaultPopupOpened
        {
            get { return _isRestoreDefaultPopupOpened; }
            set
            {
                _isRestoreDefaultPopupOpened = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="SettingsGeneralUserControlViewModel"/> class.
        /// </summary>
        internal SettingsGeneralUserControlViewModel()
        {
            if (!IsInDesignMode)
            {
                _mouseAndKeyboardHookService = ServiceLocator.GetService<MouseAndKeyboardHookService>();
            }
            _settingProvider = new ServiceSettingProvider();
            _keyboardShortcut = new List<Key>();

            InitializeCommands();

            DisplayCurrentKeyboardShortcut();
        }

        #endregion

        #region Commands

        /// <summary>
        /// Initialize the commands of the View Model
        /// </summary>
        private void InitializeCommands()
        {
            ToggleChangeHotKeysCommand = new RelayCommand(ExecuteToggleChangeHotKeysCommand);
            AcceptHotKeysCommand = new RelayCommand(ExecuteAcceptHotKeysCommand, CanExecuteAcceptHotKeysCommand);
            ChangeHotKeyPopupClosedCommand = new RelayCommand(ExecuteChangeHotKeyPopupClosedCommand);
            RestoreDefaultCommand = new RelayCommand(ExecuteRestoreDefaultCommand);
            ConfirmRestoreDefaultCommand = new RelayCommand(ExecuteConfirmRestoreDefaultCommand);
        }

        #region ChangeHotKeys

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when we click on the Change keyboard shortcut button
        /// </summary>
        public RelayCommand ToggleChangeHotKeysCommand { get; private set; }

        private void ExecuteToggleChangeHotKeysCommand()
        {
            IsChangeHotKeyPopupOpened = true;
            MessengerInstance.Send(new Message(), MessageIdentifiers.ChangeHotKeyPopupOpened);

            _keyboardShortcut.Clear();
            DisplayedTemporaryKeyboardShortcut = DisplayedCurrentKeyboardShortcut;
            AcceptHotKeysCommand.RaiseCanExecuteChanged();
            _mouseAndKeyboardHookService.KeyboardAction += MouseAndKeyboardHookService_KeyboardAction;
            _mouseAndKeyboardHookService.Resume();
        }

        #endregion

        #region AcceptHotKeys

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when we click on the Accept button of the keyboard shortcut popup
        /// </summary>
        public RelayCommand AcceptHotKeysCommand { get; private set; }

        private bool CanExecuteAcceptHotKeysCommand()
        {
            return _keyboardShortcut.Count > 0 && !_keyboardShortcut.Contains(Key.Escape) && !_keyboardShortcut.Contains(Key.Tab);
        }

        private void ExecuteAcceptHotKeysCommand()
        {
            Logger.Instance.Information($"The keyboard shortcut has been changed by the user. The new shortcut is {JsonConvert.SerializeObject(_keyboardShortcut)}.");
            IsChangeHotKeyPopupOpened = false;

            Settings.Default.KeyboardShortcut = new ArrayList(_keyboardShortcut);
            _settingProvider.SaveAndApplySettings();
            DisplayCurrentKeyboardShortcut();
        }

        #endregion

        #region ChangeHotKeyPopupClosed

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the keyboard shortcut popup is closed
        /// </summary>
        public RelayCommand ChangeHotKeyPopupClosedCommand { get; private set; }

        private void ExecuteChangeHotKeyPopupClosedCommand()
        {
            IsChangeHotKeyPopupOpened = false;
            _mouseAndKeyboardHookService.Pause();
            _mouseAndKeyboardHookService.KeyboardAction -= MouseAndKeyboardHookService_KeyboardAction;
        }

        #endregion

        #region RestoreDefault

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when we click on the Restore default button
        /// </summary>
        public RelayCommand RestoreDefaultCommand { get; private set; }

        private void ExecuteRestoreDefaultCommand()
        {
            IsRestoreDefaultPopupOpened = !IsRestoreDefaultPopupOpened;
            if (IsRestoreDefaultPopupOpened)
            {
                MessengerInstance.Send(new Message(), MessageIdentifiers.RestoreDefaultSettingsPopupOpened);
            }
        }

        #endregion

        #region ConfirmRestoreDefault

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when we click on the Confirm button of the Restore default settings popup
        /// </summary>
        public RelayCommand ConfirmRestoreDefaultCommand { get; private set; }

        private async void ExecuteConfirmRestoreDefaultCommand()
        {
            Logger.Instance.Information($"The default settings will be restored by the user.");

            var language = Settings.Default.Language;

            await ServiceLocator.GetService<CloudStorageService>().SignOutAllAsync();

            CoreHelper.UpdateStartWithWindowsShortcut(true);
            Settings.Default.Reset();

            Settings.Default.FirstStart = false;
            Settings.Default.Language = language;
            Settings.Default.KeyboardShortcut = Consts.DEFAULT_KeyboardHotKeys;
            Settings.Default.KeepDataTypes = Consts.DEFAULT_DataTypesToKeep;
            Settings.Default.IgnoredApplications = JsonConvert.SerializeObject(new ObservableCollection<IgnoredApplication>());

            _settingProvider.SaveAndApplySettings();
            IsRestoreDefaultPopupOpened = false;
            RaisePropertyChanged(string.Empty);
            MessengerInstance.Send(new Message(), MessageIdentifiers.RaisePropertyChangedOnAllSettingsUserControl);
            Logger.Instance.Information($"The default settings have been restored by the user.");
        }

        #endregion

        #endregion

        #region Handled Methods

        private void MouseAndKeyboardHookService_KeyboardAction(object sender, KeyboardHookEventArgs e)
        {
            if (e.State == KeyState.Pressed)
            {
                if (e.Key == Key.Return || e.Key == Key.Space && AcceptHotKeysCommand.CanExecute(null))
                {
                    AcceptHotKeysCommand.Execute(null);
                }
            }
            if (e.State == KeyState.Released)
            {
                if (e.Key == Key.Escape)
                {
                    ChangeHotKeyPopupClosedCommand.Execute(null);
                    return;
                }
                else if (e.Key == Key.Return || e.Key == Key.Space || e.Key == Key.Tab)
                {
                    return;
                }

                _keyboardShortcut.Add(e.Key);
                DisplayedTemporaryKeyboardShortcut = KeyToString(_keyboardShortcut);
                AcceptHotKeysCommand.RaiseCanExecuteChanged();
            }

            e.Handled = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generate a string that represents the current keyboard shortcut and display it
        /// </summary>
        private void DisplayCurrentKeyboardShortcut()
        {
            if (Settings.Default.KeyboardShortcut == null)
            {
                return;
            }

            var currentKeyboardShortcut = new List<Key>();
            foreach (var k in Settings.Default.KeyboardShortcut)
            {
                var key = (Key)k;
                currentKeyboardShortcut.Add(key);
            }

            DisplayedCurrentKeyboardShortcut = KeyToString(currentKeyboardShortcut);
        }

        /// <summary>
        /// Convert a list of <see cref="Key"/> to a <see cref="string"/>
        /// </summary>
        /// <param name="keys">The list of keys to convert</param>
        /// <returns>A <see cref="string"/> that represents the list of <see cref="Key"/></returns>
        private string KeyToString(List<Key> keys)
        {
            var result = string.Empty;

            foreach (var key in keys)
            {
                result += $"{KeyToString(key)} + ";
            }

            result = result.Substring(0, result.Length - 3);

            return result;
        }

        /// <summary>
        /// Convert a <see cref="Key"/> to a <see cref="string"/>
        /// </summary>
        /// <param name="key">The key to convert</param>
        /// <returns>A <see cref="string"/> that represents the <see cref="Key"/></returns>
        private string KeyToString(Key key)
        {
            Requires.NotNull(key, nameof(key));
            var keyString = key.ToString();
            keyString = keyString.Replace("Key", string.Empty);
            keyString = Regex.Replace(keyString, "(\\B[A-Z])", " $1");
            return keyString;
        }

        #endregion
    }
}
