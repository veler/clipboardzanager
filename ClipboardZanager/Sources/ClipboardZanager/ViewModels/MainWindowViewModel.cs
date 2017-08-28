using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ClipboardZanager.ComponentModel.Enums;
using ClipboardZanager.ComponentModel.Messages;
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
using ClipboardZanager.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using Application = System.Windows.Application;
using Message = ClipboardZanager.ComponentModel.Messages.Message;
using MessageBox = System.Windows.MessageBox;

namespace ClipboardZanager.ViewModels
{
    /// <summary>
    /// Interaction logic for <see cref="MainWindow"/>
    /// </summary>
    internal sealed class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        private readonly Delayer<object> _delayedPasteCommand;
        private readonly DispatcherTimer _timer;
        private PasteBarWindow _pasteBarWindow;
        private Visibility _notifyIconVisibility;
        private ImageSource _notifyIconSource;
        private bool _pasteBarDisplayed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="LanguageManager"/>
        /// </summary>
        public LanguageManager Language => LanguageManager.GetInstance();

        /// <summary>
        /// Gets or sets the visibility of the <see cref="NotifyIcon"/> of the <see cref="MainWindow"/>.
        /// </summary>
        public Visibility NotifyIconVisibility
        {
            get { return _notifyIconVisibility; }
            private set
            {
                _notifyIconVisibility = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the icon of the <see cref="NotifyIcon"/> of the <see cref="MainWindow"/>.
        /// </summary>
        public ImageSource NotifyIconSource
        {
            get { return _notifyIconSource; }
            private set
            {
                _notifyIconSource = value;
                RaisePropertyChanged();
            }
        }

        #endregion 

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        internal MainWindowViewModel()
        {
            InitializeCommands();

            if (IsInDesignMode)
            {
                return;
            }
 
            var firstStart = Settings.Default.FirstStart;
            if (firstStart)
            {
                var dataMigrationRequired = Settings.Default.DataMigrationRequired;
                var previousVersion = Settings.Default.CurrentVersion;
                Settings.Default.Upgrade();
                Settings.Default.DataMigrationRequired = dataMigrationRequired;
                if (!string.IsNullOrWhiteSpace(previousVersion))
                {
                    Settings.Default.CurrentVersion = previousVersion;
                }

                var firstStartWindow = new FirstStartWindow();
                firstStartWindow.ShowDialog();

                Settings.Default.FirstStart = false;
                Settings.Default.Save();
                Settings.Default.Reload();
            }

            // First call and initialization of services.
            var dataService = ServiceLocator.GetService<DataService>();
            var cloudStorageService = ServiceLocator.GetService<CloudStorageService>();
            var mouseAndKeyboardHookService = ServiceLocator.GetService<MouseAndKeyboardHookService>();
            ServiceLocator.GetService<WindowsService>();
            ServiceLocator.GetService<ClipboardService>();

            dataService.CreditCardNumberDetected += DataService_CreditCardNumberDetected;
            dataService.CreditCardNumberSaved += DataService_CreditCardNumberSaved;
            dataService.PasswordDetected += DataService_PasswordDetected;
            dataService.PasswordSaved += DataService_PasswordSaved;
            cloudStorageService.SynchronizationStarted += CloudStorageService_SynchronizationStarted;
            cloudStorageService.SynchronizationFailed += CloudStorageService_SynchronizationFailed;
            cloudStorageService.SynchronizationEnded += CloudStorageService_SynchronizationEnded;

            _timer = new DispatcherTimer();
            _timer.Tick += TimerOnTick;
            _timer.Interval = TimeSpan.FromMilliseconds(5000);
            _timer.Start();

            _delayedPasteCommand = new Delayer<object>(TimeSpan.FromMilliseconds(10));
            _delayedPasteCommand.Action += DelayedPasteCommand;

            mouseAndKeyboardHookService.Pause();
            _pasteBarWindow = new PasteBarWindow();
            _pasteBarWindow.Show();
            mouseAndKeyboardHookService.DelayedResume(TimeSpan.FromSeconds(1));

            Messenger.Default.Register<Message>(this, MessageIdentifiers.PasteData, PasteData);

            SetNormalNotifyIcon();
            ShowNotifyIcon();

            if (firstStart)
            {
                var delayedNotification = new Delayer<object>(TimeSpan.FromSeconds(2));
                delayedNotification.Action += (sender, args) => MessengerInstance.Send(new Message(Language.FirstStartWindow.Welcome_Title, Language.FirstStartWindow.Welcome_Text), MessageIdentifiers.ShowNotifyIconBalloon);
                delayedNotification.ResetAndTick();
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Initialize the commands of the View Model
        /// </summary>
        private void InitializeCommands()
        {
            PasteCommand = new RelayCommand(ExecutePasteCommand, CanExecutePasteCommand);
            SynchronizeCommand = new RelayCommand(ExecuteSynchronizeCommand);
            SettingsCommand = new RelayCommand<SettingsViewMode>(ExecuteSettingsCommand);
            ExitCommand = new RelayCommand(ExecuteExitCommand);
            ContextMenuClosedCommand = new RelayCommand(ExecuteContextMenuClosedCommand);
            ContextMenuOpeningCommand = new RelayCommand(ExecuteContextMenuOpeningCommand);
        }

        #region Paste

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when we click on the Paste button
        /// </summary>
        public RelayCommand PasteCommand { get; private set; }

        private bool CanExecutePasteCommand()
        {
            return !_pasteBarDisplayed;
        }

        private void ExecutePasteCommand()
        {
            _pasteBarDisplayed = true;
            Logger.Instance.Information("Paste menu from icon in the task bar clicked (or mouse gesture or keyboard shortcut detected). The paste bar will be displayed.");
            HideNotifyIcon();

            _pasteBarWindow.DisplayBar(() =>
            {
                Logger.Instance.Information("The paste bar is closed.");
                ShowNotifyIcon();
                _pasteBarDisplayed = false;
            });
        }

        #endregion

        #region Synchronize

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when we click on the Synchronize now button
        /// </summary>
        public RelayCommand SynchronizeCommand { get; private set; }

        private void ExecuteSynchronizeCommand()
        {
            Logger.Instance.Information("Synchronize now menu from icon in the task bar clicked.");

            var cloudStorageService = ServiceLocator.GetService<CloudStorageService>();
            if (cloudStorageService.IsLinkedToAService)
            {
                if (!cloudStorageService.IsSynchronizing)
                {
                    cloudStorageService.SynchronizeAsync();
                }
            }
            else
            {
                SettingsCommand.Execute(SettingsViewMode.Synchronization);
            }
        }

        #endregion

        #region Settings

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when we click on the Settings button
        /// </summary>
        public RelayCommand<SettingsViewMode> SettingsCommand { get; private set; }

        private void ExecuteSettingsCommand(SettingsViewMode viewMode)
        {
            Logger.Instance.Information("Settings menu from icon in the task bar clicked. The settings window will be displayed");
            HideNotifyIcon();

            _pasteBarWindow.Close();

            var window = new SettingsWindow(viewMode);
            window.ShowDialog();

            _pasteBarWindow = new PasteBarWindow();
            _pasteBarWindow.Show();

            Logger.Instance.Information("The settings window is closed.");
            ShowNotifyIcon();

            ServiceLocator.GetService<ClipboardService>().Reset();
            ServiceLocator.GetService<CloudStorageService>().Reset();
        }

        #endregion

        #region Exit

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when we click on the Quit button
        /// </summary>
        public RelayCommand ExitCommand { get; private set; }

        private void ExecuteExitCommand()
        {
            Logger.Instance.Information("Exit menu from icon in the task bar clicked.");
            HideNotifyIcon();

            var delayer = new Delayer<object>(TimeSpan.FromMilliseconds(300));
            delayer.Action += (o, args) =>
            {
                if (MessageBox.Show(Language.MainWindow.Message_Quit, Language.MainWindow.ApplicationTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (CoreHelper.IsUnitTesting())
                    {
                        throw new OperationCanceledException("Unable to quit a unit test");
                    }

                    _pasteBarWindow.Close();
                    Application.Current.Shutdown(0);
                }
                else
                {
                    Logger.Instance.Information("Exit has been canceled.");
                    ShowNotifyIcon();
                }
            };
            delayer.ResetAndTick();
        }

        #endregion

        #region ContextMenuClosed

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the context menu is closed
        /// </summary>
        public RelayCommand ContextMenuClosedCommand { get; private set; }

        private void ExecuteContextMenuClosedCommand()
        {
            CoreHelper.MinimizeFootprint();
        }

        #endregion

        #region ContextMenuOpening

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the context menu is opening
        /// </summary>
        public RelayCommand ContextMenuOpeningCommand { get; private set; }

        private void ExecuteContextMenuOpeningCommand()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        #endregion

        #region Handled Methods

        private void MouseAndKeyboardHookService_MouseAction(object sender, MouseHookEventArgs e)
        {
            switch (Settings.Default.PasteBarPosition)
            {
                case PasteBarPosition.Top:
                    if (e.WheelAction == MouseWheelAction.WheelDown)
                    {
                        var activeScreen = Screen.FromPoint(new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
                        var screen = SystemInfoHelper.GetAllScreenInfos().Single(s => s.DeviceName == activeScreen.DeviceName);

                        if (e.Coords.Y <= screen.Bounds.Top + 5 && PasteCommand.CanExecute(null))
                        {
                            e.Handled = true;
                            Logger.Instance.Information($"Mouse gesture detected at the top of the screen.");
                            _delayedPasteCommand.ResetAndTick();
                        }
                    }
                    break;

                case PasteBarPosition.Bottom:
                    if (e.WheelAction == MouseWheelAction.WheelUp)
                    {
                        var activeScreen = Screen.FromPoint(new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
                        var screen = SystemInfoHelper.GetAllScreenInfos().Single(s => s.DeviceName == activeScreen.DeviceName);

                        if (e.Coords.Y >= screen.Bounds.Bottom + screen.Bounds.Top - 5 && PasteCommand.CanExecute(null))
                        {
                            e.Handled = true;
                            Logger.Instance.Information($"Mouse gesture detected at the bottom of the screen.");
                            _delayedPasteCommand.ResetAndTick();
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void MouseAndKeyboardHookService_HotKeyDetected(object sender, HotKeyEventArgs e)
        {
            if (PasteCommand.CanExecute(null))
            {
                Logger.Instance.Information($"The keyboard shortcut has hit.");
                e.Handled = true;
                _delayedPasteCommand.ResetAndTick();
            }
        }

        private void DataService_CreditCardNumberDetected(object sender, EventArgs e)
        {
            Logger.Instance.Information($"A credit card number has been detected.");
            if (Settings.Default.NotifyCreditCard)
            {
                if (Settings.Default.AvoidCreditCard)
                {
                    MessengerInstance.Send(new Message(Language.MainWindow.CreditCardDetected_Title, Language.MainWindow.CreditCardDetected_Text), MessageIdentifiers.ShowNotifyIconBalloon);
                }
                else
                {
                    MessengerInstance.Send(new Message(Language.MainWindow.CreditCardDetectedButSaved_Title, Language.MainWindow.CreditCardDetectedButSaved_Text), MessageIdentifiers.ShowNotifyIconBalloon);
                }
            }
        }

        private void DataService_CreditCardNumberSaved(object sender, EventArgs e)
        {
            Logger.Instance.Information($"A credit card number has been saved.");
            if (Settings.Default.NotifyCreditCard)
            {
                MessengerInstance.Send(new Message(Language.MainWindow.CreditCardSaved_Title, Language.MainWindow.CreditCardSaved_Text), MessageIdentifiers.ShowNotifyIconBalloon);
            }
        }

        private void DataService_PasswordDetected(object sender, EventArgs e)
        {
            Logger.Instance.Information($"A password has been detected.");
            if (Settings.Default.NotifyPassword)
            {
                if (Settings.Default.AvoidPasswords)
                {
                    MessengerInstance.Send(new Message(Language.MainWindow.PasswordDetected_Title, Language.MainWindow.PasswordDetected_Text), MessageIdentifiers.ShowNotifyIconBalloon);
                }
                else
                {
                    MessengerInstance.Send(new Message(Language.MainWindow.PasswordDetectedButSaved_Title, Language.MainWindow.PasswordDetectedButSaved_Text), MessageIdentifiers.ShowNotifyIconBalloon);
                }
            }
        }

        private void DataService_PasswordSaved(object sender, EventArgs e)
        {
            Logger.Instance.Information($"A password has been saved.");
            if (Settings.Default.NotifyPassword)
            {
                MessengerInstance.Send(new Message(Language.MainWindow.PasswordSaved_Title, Language.MainWindow.PasswordSaved_Text), MessageIdentifiers.ShowNotifyIconBalloon);
            }
        }

        private void CloudStorageService_SynchronizationFailed(object sender, EventArgs e)
        {
            Logger.Instance.Information($"The synchronization has failed.");

            if (Settings.Default.NotifySyncFailed)
            {
                MessengerInstance.Send(new Message(Language.MainWindow.SynchronizationFailed_Title, Language.MainWindow.SynchronizationFailed_Text), MessageIdentifiers.ShowNotifyIconBalloon);
            }
        }

        private void CloudStorageService_SynchronizationEnded(object sender, EventArgs e)
        {
            SetNormalNotifyIcon();
        }

        private void CloudStorageService_SynchronizationStarted(object sender, EventArgs e)
        {
            SetSynchronizingNotifyIcon();
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            CoreHelper.MinimizeFootprint();
        }

        private void DelayedPasteCommand(object sender, DelayerActionEventArgs<object> e)
        {
            if (PasteCommand.CanExecute(null))
            {
                PasteCommand.Execute(null);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Hide the <see cref="ComponentModel.UI.Controls.NotifyIcon"/> and disable the mouse and keyboard interaction.
        /// </summary>
        private void HideNotifyIcon()
        {
            DispatcherHelper.ThrowIfNotStaThread();

            NotifyIconVisibility = Visibility.Hidden;

            var mouseAndKeyboardHookService = ServiceLocator.GetService<MouseAndKeyboardHookService>();
            mouseAndKeyboardHookService.Pause();
            ServiceLocator.GetService<ClipboardService>().Pause();

            if (Settings.Default.UseMouseGesture)
            {
                mouseAndKeyboardHookService.MouseAction -= MouseAndKeyboardHookService_MouseAction;
            }

            if (Settings.Default.UseKeyboardHotKeys)
            {
                mouseAndKeyboardHookService.UnregisterHotKey(Consts.PasteShortcutName);
                mouseAndKeyboardHookService.HotKeyDetected -= MouseAndKeyboardHookService_HotKeyDetected;
            }

            Logger.Instance.Information($"The icon in the task bar is hidden.");
        }

        /// <summary>
        /// Show the <see cref="ComponentModel.UI.Controls.NotifyIcon"/> and enable the mouse and keyboard interaction.
        /// </summary>
        private void ShowNotifyIcon()
        {
            DispatcherHelper.ThrowIfNotStaThread();

            NotifyIconVisibility = Visibility.Visible;

            var mouseAndKeyboardHookService = ServiceLocator.GetService<MouseAndKeyboardHookService>();

            if (Settings.Default.UseMouseGesture)
            {
                mouseAndKeyboardHookService.MouseAction += MouseAndKeyboardHookService_MouseAction;
                Logger.Instance.Information($"The mouse is listened.");
            }

            if (Settings.Default.UseKeyboardHotKeys)
            {
                mouseAndKeyboardHookService.HotKeyDetected += MouseAndKeyboardHookService_HotKeyDetected;
                mouseAndKeyboardHookService.RegisterHotKey(Consts.PasteShortcutName, Settings.Default.KeyboardShortcut.Cast<Key>().ToArray());
                Logger.Instance.Information($"The keyboard is listened. The expecting shortcut is {JsonConvert.SerializeObject(Settings.Default.KeyboardShortcut.Cast<Key>().ToArray())}.");
            }

            ServiceLocator.GetService<ClipboardService>().Resume();
            mouseAndKeyboardHookService.DelayedResume(TimeSpan.FromMilliseconds(1000));
            Logger.Instance.Information($"The icon in the task bar is shown.");
        }

        /// <summary>
        /// Change the <see cref="ComponentModel.UI.Controls.NotifyIcon"/>'s icon to the normal icon.
        /// </summary>
        private void SetNormalNotifyIcon()
        {
            var icon = new BitmapImage();
            icon.BeginInit();
            icon.UriSource = new Uri(@"pack://application:,,,/ClipboardZanager;component/Assets/paste.ico");
            icon.EndInit();
            NotifyIconSource = icon;
        }

        /// <summary>
        /// Change the <see cref="ComponentModel.UI.Controls.NotifyIcon"/>'s icon to the icon designed for the synchronization.
        /// </summary>
        private void SetSynchronizingNotifyIcon()
        {
            var icon = new BitmapImage();
            icon.BeginInit();
            icon.UriSource = new Uri(@"pack://application:,,,/ClipboardZanager;component/Assets/paste-sync.ico");
            icon.EndInit();
            NotifyIconSource = icon;
        }

        /// <summary>
        /// Paste the data to the current foreground window.
        /// </summary>
        /// <param name="message">The message to show.</param>
        private void PasteData(Message message)
        {
            Requires.NotNull(message.Values, nameof(message.Values));

            var delayer = new Delayer<object>(TimeSpan.FromMilliseconds(200));
            delayer.Action += (sender, args) =>
            {
                var clipboardService = ServiceLocator.GetService<ClipboardService>();
                var mouseAndKeyboardHookService = ServiceLocator.GetService<MouseAndKeyboardHookService>();
                mouseAndKeyboardHookService.Pause();
                clipboardService.Pause();

                ServiceLocator.GetService<DataService>().CopyData((DataEntry)message.Values.First());
                clipboardService.Paste();

                delayer = new Delayer<object>(TimeSpan.FromMilliseconds(300));
                delayer.Action += (sender2, args2) =>
                {
                    mouseAndKeyboardHookService.Resume();
                    clipboardService.Resume();
                };
                delayer.ResetAndTick();
            };
            delayer.ResetAndTick();
        }

        #endregion
    }
}
